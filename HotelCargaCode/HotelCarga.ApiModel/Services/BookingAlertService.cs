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
    private const byte QUEUE_STATUS_PENDING = 1;      // Pending
    private const byte QUEUE_STATUS_NOTIFIED = 2;     // Notified

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

                var notification = new WaitingQueueNotification
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
                    Position = i + 1, // Position in FIFO queue
                    CreatedAt = queueEntry.created_at ?? DateTime.MinValue,
                    Message = $"A room in the {queueEntry.room_category?.category_name} category is now available for {queueEntry.requested_check_in:yyyy-MM-dd}. Please confirm if you're still interested."
                };

                notifications.Add(notification);
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
                notifications.Add(new WaitingQueueNotification
                {
                    WaitingQueueId = alert.id,
                    CustomerId = alert.customer_id,
                    CustomerName = alert.customer != null 
                        ? $"{alert.customer.first_name} {alert.customer.last_name}" 
                        : "Unknown",
                    RoomCategoryId = alert.room_category_id,
                    RoomCategoryName = alert.room_category?.category_name ?? "Unknown",
                    RequestedCheckIn = alert.requested_check_in,
                    RequestedCheckOut = alert.check_out ?? DateTime.MinValue,
                    Position = i + 1,
                    CreatedAt = alert.created_at ?? DateTime.MinValue
                });
            }

            return notifications;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending alerts");
            return new List<WaitingQueueNotification>();
        }
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
