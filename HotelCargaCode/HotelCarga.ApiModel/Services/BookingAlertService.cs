using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelCargaContext = HotelCarga.DbModel.HotelCargaContext;
using HotelCarga.HotelCarga.DbModel.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelCarga.ApiModel.Services;

/// <summary>
/// Service responsible for managing alerts and FIFO notifications when booking changes occur.
/// Handles alerts when rooms are released due to cancellation or rescheduling.
/// </summary>
public class BookingAlertService
{
    private readonly HotelCargaContext _dbContext;
    private readonly ILogger<BookingAlertService> _logger;

    // Status IDs must match seeded database values.
    private const byte QUEUE_STATUS_PENDING = 1;
    private const byte QUEUE_STATUS_CONFIRMED = 2;
    private const byte BOOKING_STATUS_CONFIRMED = 2;
    private const byte BOOKING_STATUS_PENDING = 1;

    public BookingAlertService(HotelCargaContext dbContext, ILogger<BookingAlertService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Generates alerts when a room is released due to booking cancellation or rescheduling.
    /// Contacts waiting queue customers in FIFO order (first-in-first-out by created_at).
    /// </summary>
    public async Task<List<WaitingQueueNotification>> GenerateAlertsForReleasedRoom(uint roomId, DateTime checkInDate, DateTime checkOutDate)
    {
        var notifications = new List<WaitingQueueNotification>();

        try
        {
            // Get the room to find its category
            var room = await _dbContext.Set<room>().FindAsync(roomId);
            if (room == null)
            {
                _logger.LogWarning($"Room {roomId} not found when generating alerts");
                return notifications;
            }

            byte roomCategoryId = room.category_id;

            // Find waiting queue entries for this room category that match the date range
            // Order by created_at (FIFO) and filter by PENDING status
            var waitingQueueEntries = await _dbContext.Set<waiting_queue>()
                .Where(wq => wq.room_category_id == roomCategoryId
                    && wq.status_id == QUEUE_STATUS_PENDING
                    && wq.requested_check_in >= checkInDate
                    && (wq.check_out == null || wq.check_out <= checkOutDate))
                .OrderBy(wq => wq.created_at) // FIFO ordering
                .Include(wq => wq.customer)
                .Include(wq => wq.room_category)
                .ToListAsync();

            if (waitingQueueEntries.Count == 0)
            {
                _logger.LogInformation($"No waiting queue entries found for room category {roomCategoryId}");
                return notifications;
            }

            // Create notifications for the first few entries (limit to 5 per alert batch)
            int notificationLimit = Math.Min(5, waitingQueueEntries.Count);
            for (int i = 0; i < notificationLimit; i++)
            {
                var queueEntry = waitingQueueEntries[i];

                // Update the queue entry status to NOTIFIED
                queueEntry.status_id = QUEUE_STATUS_CONFIRMED;
                queueEntry.updated_at = DateTime.UtcNow;

                notifications.Add(BuildNotification(queueEntry, roomCategoryId, i + 1));
            }

            // Save all changes
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation($"Generated {notifications.Count} alerts for room category {roomCategoryId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating alerts for released room");
        }

        return notifications;
    }

    /// <summary>
    /// Assigns the freed room to the oldest FIFO waiting-queue request whose requested range
    /// fits inside the released availability window. If a matching pending booking exists,
    /// it is confirmed in-place; otherwise a confirmed booking is created as a recovery path.
    /// The processed waiting-queue row is deleted.
    /// </summary>
    public async Task<WaitingQueueNotification?> ProcessNextQueueForReleasedRoom(uint roomId, DateTime availableFrom, DateTime availableTo)
    {
        var room = await _dbContext.Set<room>()
            .Include(r => r.category)
            .FirstOrDefaultAsync(r => r.id == roomId);

        if (room == null)
        {
            _logger.LogWarning("Room {roomId} not found when processing availability cascade", roomId);
            return null;
        }

        if (availableTo <= availableFrom)
        {
            _logger.LogInformation("Released availability window for room {roomId} is empty", roomId);
            return null;
        }

        var candidateEntries = await _dbContext.Set<waiting_queue>()
            .Where(wq => wq.room_category_id == room.category_id
                && wq.status_id == QUEUE_STATUS_PENDING
                && wq.requested_check_in >= availableFrom
                && (wq.check_out ?? wq.requested_check_in.AddDays(1)) <= availableTo)
            .OrderBy(wq => wq.created_at)
            .ThenBy(wq => wq.id)
            .Include(wq => wq.customer)
            .Include(wq => wq.room_category)
            .ToListAsync();

        if (candidateEntries.Count == 0)
        {
            _logger.LogInformation(
                "No pending queue entry fits released room {roomId} between {availableFrom} and {availableTo}",
                roomId,
                availableFrom,
                availableTo);
            return null;
        }

        foreach (var queueEntry in candidateEntries)
        {
            var requestedCheckOut = queueEntry.check_out ?? queueEntry.requested_check_in.AddDays(1);
            var overlapsExistingReservation = await _dbContext.Set<booking>()
                .AnyAsync(b => b.room_id == roomId
                    && b.status_id != 3
                    && b.check_in < requestedCheckOut
                    && b.check_out > queueEntry.requested_check_in);

            if (overlapsExistingReservation)
            {
                continue;
            }

            var pendingBooking = await _dbContext.Set<booking>()
                .Where(b => b.customer_id == queueEntry.customer_id
                    && b.status_id == BOOKING_STATUS_PENDING
                    && b.room_id == null
                    && b.check_in.Date == queueEntry.requested_check_in.Date
                    && b.check_out.Date == requestedCheckOut.Date)
                .OrderBy(b => b.created_at)
                .ThenBy(b => b.id)
                .FirstOrDefaultAsync();

            if (pendingBooking == null)
            {
                pendingBooking = new booking
                {
                    reserve_number = await GeneratePendingReserveNumberAsync(),
                    customer_id = queueEntry.customer_id,
                    room_id = roomId,
                    status_id = BOOKING_STATUS_CONFIRMED,
                    check_in = queueEntry.requested_check_in,
                    check_out = requestedCheckOut,
                    nightly_rate = room.nightly_rate,
                    total_price = CalculateTotalPrice(room.nightly_rate, queueEntry.requested_check_in, requestedCheckOut),
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                };

                await _dbContext.Set<booking>().AddAsync(pendingBooking);
            }
            else
            {
                pendingBooking.room_id = roomId;
                pendingBooking.status_id = BOOKING_STATUS_CONFIRMED;
                pendingBooking.nightly_rate = room.nightly_rate;
                pendingBooking.total_price = CalculateTotalPrice(room.nightly_rate, pendingBooking.check_in, pendingBooking.check_out);
                pendingBooking.updated_at = DateTime.UtcNow;
            }

            room.status_id = BOOKING_STATUS_CONFIRMED;
            _dbContext.Set<waiting_queue>().Remove(queueEntry);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Availability cascade promoted queue entry {queueId} for customer {customerId} into booking {bookingId} on room {roomId}",
                queueEntry.id,
                queueEntry.customer_id,
                pendingBooking.id,
                roomId);

            return BuildNotification(queueEntry, room.category_id, 1);
        }

        _logger.LogInformation(
            "Released room {roomId} had FIFO candidates, but none could be assigned after conflict checks",
            roomId);
        return null;
    }

    /// <summary>
    /// Retrieves pending alerts (notified customers in FIFO order) for manual review by operators.
    /// </summary>
    public async Task<List<WaitingQueueNotification>> GetPendingAlertsForOperator()
    {
        try
        {
            var pendingAlerts = await _dbContext.Set<waiting_queue>()
                .Where(wq => wq.status_id == QUEUE_STATUS_CONFIRMED)
                .OrderBy(wq => wq.created_at) // FIFO order
                .Include(wq => wq.customer)
                .Include(wq => wq.room_category)
                .ToListAsync();

            var notifications = new List<WaitingQueueNotification>();
            for (int i = 0; i < pendingAlerts.Count; i++)
            {
                var alert = pendingAlerts[i];
                notifications.Add(BuildNotification(alert, alert.room_category_id, i + 1));
            }

            return notifications;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending alerts");
            return new List<WaitingQueueNotification>();
        }
    }

    private static WaitingQueueNotification BuildNotification(waiting_queue queueEntry, byte roomCategoryId, int position)
    {
        return new WaitingQueueNotification
        {
            WaitingQueueId = queueEntry.id,
            CustomerId = queueEntry.customer_id,
            CustomerName = queueEntry.customer != null
                ? $"{queueEntry.customer.first_name} {queueEntry.customer.last_name}"
                : "Unknown",
            RoomCategoryId = roomCategoryId,
            RoomCategoryName = queueEntry.room_category?.category_name ?? "Unknown",
            RequestedCheckIn = queueEntry.requested_check_in,
            RequestedCheckOut = queueEntry.check_out ?? DateTime.MinValue,
            Position = position,
            CreatedAt = queueEntry.created_at ?? DateTime.MinValue,
            Message = $"A room in the {queueEntry.room_category?.category_name} category is now available for {queueEntry.requested_check_in:yyyy-MM-dd}. Please confirm if you're still interested."
        };
    }

    private async Task<string> GeneratePendingReserveNumberAsync()
    {
        var lastReservation = await _dbContext.bookings
            .OrderByDescending(b => b.reserve_number)
            .Select(b => b.reserve_number)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(lastReservation))
        {
            return "RES0001";
        }

        if (!lastReservation.StartsWith("RES", StringComparison.OrdinalIgnoreCase))
        {
            return $"RES{DateTime.UtcNow:yyyyMMddHHmmss}";
        }

        var numericPart = lastReservation[3..];
        return int.TryParse(numericPart, out var number)
            ? $"RES{number + 1:D4}"
            : $"RES{DateTime.UtcNow:yyyyMMddHHmmss}";
    }

    private static decimal CalculateTotalPrice(decimal nightlyRate, DateTime checkIn, DateTime checkOut)
    {
        var nights = (checkOut.Date - checkIn.Date).Days;
        if (nights <= 0)
        {
            nights = 1;
        }

        return nightlyRate * nights;
    }
}

/// <summary>
/// DTO for waiting queue notifications sent to customers
/// </summary>
public class WaitingQueueNotification
{
    public uint WaitingQueueId { get; set; }
    public uint CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public byte RoomCategoryId { get; set; }
    public string RoomCategoryName { get; set; } = string.Empty;
    public DateTime RequestedCheckIn { get; set; }
    public DateTime RequestedCheckOut { get; set; }
    public int Position { get; set; } // Position in FIFO queue
    public DateTime CreatedAt { get; set; }
    public string? Message { get; set; }
}
