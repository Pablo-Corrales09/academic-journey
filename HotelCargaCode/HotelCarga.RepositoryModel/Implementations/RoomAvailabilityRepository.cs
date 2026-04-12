using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelCarga.RepositoryModel.Interfaces;
using Microsoft.EntityFrameworkCore;
using Entities = HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCarga.RepositoryModel.Implementations;

/// <summary>
/// Implementation of IRoomAvailabilityRepository
/// </summary>
public class RoomAvailabilityRepository : IRoomAvailabilityRepository
{
    private readonly Entities.HotelCargaContext _context;

    public RoomAvailabilityRepository(Entities.HotelCargaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Entities.room_availability?> GetByIdAsync(uint id)
    {
        return await _context.room_availabilities
            .Include(ra => ra.room)
            .FirstOrDefaultAsync(ra => ra.id == id);
    }

    public async Task<IEnumerable<Entities.room_availability>> GetByRoomIdAsync(uint roomId)
    {
        return await _context.room_availabilities
            .Where(ra => ra.room_id == roomId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.room_availability>> GetByScheduleRangeAsync(DateTime startSchedule, DateTime endSchedule)
    {
        return await _context.room_availabilities
            .Where(ra => ra.start_schedule >= startSchedule && ra.end_schedule <= endSchedule)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.room_availability>> GetOverlappingAvailabilitiesAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.room_availabilities
            .Where(ra => ra.start_schedule < endDate && ra.end_schedule > startDate)
            .ToListAsync();
    }

    public async Task<Entities.room?> GetRoomByAvailabilityIdAsync(uint availabilityId)
    {
        var availability = await _context.room_availabilities
            .Include(ra => ra.room)
            .FirstOrDefaultAsync(ra => ra.id == availabilityId);
        
        return availability?.room;
    }

    public async Task<IEnumerable<Entities.room_availability>> GetAllAvailabilitiesAsync()
    {
        return await _context.room_availabilities.ToListAsync();
    }
}
