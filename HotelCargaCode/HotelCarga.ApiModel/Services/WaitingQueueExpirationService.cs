using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using HotelCargaContext = HotelCarga.DbModel.HotelCargaContext;
using HotelCarga.HotelCarga.DbModel.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelCarga.ApiModel.Services;

/// <summary>
/// Background service that handles waiting queue expiration.
/// Automatically marks waiting queue entries as "Discarded" if the requested date has passed
/// and no room was released for booking.
/// </summary>
public class WaitingQueueExpirationService : BackgroundService
{
    private readonly ILogger<WaitingQueueExpirationService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6); // Check every 6 hours

    // Queue status IDs (should match database values)
    private const byte QUEUE_STATUS_PENDING = 1;      // Pending
    private const byte QUEUE_STATUS_DISCARDED = 4;    // Discarded

    public WaitingQueueExpirationService(ILogger<WaitingQueueExpirationService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("WaitingQueueExpirationService starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExpireWaitingQueueEntries(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in WaitingQueueExpirationService");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("WaitingQueueExpirationService stopping.");
    }

    private async Task ExpireWaitingQueueEntries(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HotelCargaContext>();

            try
            {
                DateTime today = DateTime.UtcNow.Date;

                // Find all pending waiting queue entries where the requested check-in date has passed
                var expiredEntries = await dbContext.Set<waiting_queue>()
                    .Where(wq => wq.status_id == QUEUE_STATUS_PENDING && wq.requested_check_in.Date < today)
                    .ToListAsync(cancellationToken);

                if (expiredEntries.Count > 0)
                {
                    foreach (var entry in expiredEntries)
                    {
                        entry.status_id = QUEUE_STATUS_DISCARDED;
                        entry.updated_at = DateTime.UtcNow;
                    }

                    await dbContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation($"Expired {expiredEntries.Count} waiting queue entries");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error expiring waiting queue entries");
                throw;
            }
        }
    }
}
