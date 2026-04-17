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

    // Queue status IDs (should match database values)
    private const byte QUEUE_STATUS_PENDING = 1;
    private const byte QUEUE_STATUS_NOTIFIED = 2;
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
                queueEntry.status_id = QUEUE_STATUS_NOTIFIED;
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
    /// Assigns the freed room to the oldest pending waiting-queue request for the same room category.
    /// Creates the reservation and updates queue status in a single transaction.
    /// </summary>
    public async Task<WaitingQueueNotification?> ProcessNextQueueForReleasedRoom(uint roomId, DateTime checkInDate, DateTime checkOutDate)
    {
        try
        {
            var room = await _dbContext.Set<room>().FindAsync(roomId);
            if (room == null)
            {
                _logger.LogWarning($"Room {roomId} not found when processing next waiting queue entry");
                return null;
            }

            byte roomCategoryId = room.category_id;

            var candidateEntries = await _dbContext.Set<waiting_queue>()
                .Where(wq => wq.room_category_id == roomCategoryId
                    && wq.status_id == QUEUE_STATUS_PENDING
                    && wq.requested_check_in >= checkInDate
                    && (wq.check_out == null || wq.check_out <= checkOutDate))
                .OrderBy(wq => wq.created_at)
                .Include(wq => wq.customer)
                .Include(wq => wq.room_category)
                .ToListAsync();

            if (candidateEntries.Count == 0)
            {
                _logger.LogInformation($"No pending queue entry available for released room {roomId}");
                return null;
            }

            var nextQueueEntry = candidateEntries.First();

            // Guard: only assign if the freed room category matches the queued room category.
            if (nextQueueEntry.room_category_id != roomCategoryId)
            {
                _logger.LogInformation($"Queue entry {nextQueueEntry.id} category does not match freed room {roomId} category");
                return null;
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var targetCheckOut = nextQueueEntry.check_out ?? nextQueueEntry.requested_check_in.AddDays(1);

            // Prevent duplicate reservations for the same queue request if this method is invoked more than once.
            var existingBooking = await _dbContext.Set<booking>()
                .Where(b => b.customer_id == nextQueueEntry.customer_id
                    && b.room_id == roomId
                    && b.check_in.Date == nextQueueEntry.requested_check_in.Date
                    && b.check_out.Date == targetCheckOut.Date
                    && (b.status_id == BOOKING_STATUS_PENDING || b.status_id == BOOKING_STATUS_CONFIRMED))
                .OrderByDescending(b => b.id)
                .FirstOrDefaultAsync();

            if (existingBooking == null)
            {
                var newBooking = new booking
                {
                    reserve_number = await GeneratePendingReserveNumberAsync(),
                    customer_id = nextQueueEntry.customer_id,
                    room_id = roomId,
                    status_id = BOOKING_STATUS_PENDING,
                    check_in = nextQueueEntry.requested_check_in,
                    check_out = targetCheckOut,
                    nightly_rate = room.nightly_rate,
                    total_price = CalculateTotalPrice(room.nightly_rate, nextQueueEntry.requested_check_in, targetCheckOut),
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                };

                await _dbContext.Set<booking>().AddAsync(newBooking);
            }

            nextQueueEntry.status_id = QUEUE_STATUS_NOTIFIED;
            nextQueueEntry.updated_at = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation($"Queue entry {nextQueueEntry.id} assigned to room {roomId}; booking created and queue marked NOTIFIED");
            return BuildNotification(nextQueueEntry, roomCategoryId, 1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing next queue entry for released room");
            return null;
        }
    }

    /// <summary>
    /// Retrieves pending alerts (notified customers in FIFO order) for manual review by operators.
    /// </summary>
    public async Task<List<WaitingQueueNotification>> GetPendingAlertsForOperator()
    {
        try
        {
            var pendingAlerts = await _dbContext.Set<waiting_queue>()
                .Where(wq => wq.status_id == QUEUE_STATUS_NOTIFIED)
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
