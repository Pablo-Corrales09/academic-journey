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
/// Background service that checks room availability for a 15-day schedule.
/// Runs periodically to ensure room availability records are up-to-date.
/// </summary>
public class RoomAvailabilitySchedulerService : BackgroundService
{
    private readonly ILogger<RoomAvailabilitySchedulerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every hour
    private const int AvailabilityRangeInDays = 15;

    public RoomAvailabilitySchedulerService(ILogger<RoomAvailabilitySchedulerService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RoomAvailabilitySchedulerService starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateRoomAvailabilitySchedule(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in RoomAvailabilitySchedulerService");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("RoomAvailabilitySchedulerService stopping.");
    }

    private async Task UpdateRoomAvailabilitySchedule(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HotelCargaContext>();

            try
            {
                DateTime today = DateTime.UtcNow.Date;
                DateTime endOfRange = today.AddDays(AvailabilityRangeInDays);

                // Get all rooms
                var rooms = await dbContext.Set<room>().ToListAsync(cancellationToken);

                foreach (var room in rooms)
                {
                    // Check if availability records exist for the 15-day range
                    var existingAvailabilities = await dbContext.Set<room_availability>()
                        .Where(a => a.room_id == room.id && a.start_schedule >= today && a.end_schedule <= endOfRange)
                        .ToListAsync(cancellationToken);

                    // If room is available and no record exists for today, create one
                    if (room.status_id == 1) // 1 = Available
                    {
                        var hasRecordForToday = existingAvailabilities.Any(a => a.start_schedule <= today && a.end_schedule >= today);
                        
                        if (!hasRecordForToday)
                        {
                            var availability = new room_availability
                            {
                                room_id = room.id,
                                start_schedule = today,
                                end_schedule = endOfRange,
                                creation_at = DateTime.UtcNow
                            };

                            await dbContext.Set<room_availability>().AddAsync(availability, cancellationToken);
                        }
                    }
                }

                await dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation($"Room availability schedule updated for {AvailabilityRangeInDays} days");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating room availability schedule");
                throw;
            }
        }
    }
}
