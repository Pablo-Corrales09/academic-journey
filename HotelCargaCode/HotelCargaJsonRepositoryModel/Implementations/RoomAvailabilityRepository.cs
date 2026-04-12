using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelCargaJsonRepositoryModel.Interfaces;
using Entities = HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCargaJsonRepositoryModel.Implementations;

/// <summary>
/// Implementation of IRoomAvailabilityRepository
/// </summary>
public class RoomAvailabilityRepository : IRoomAvailabilityRepository
{
    private readonly JsonDataContext _context;

    public RoomAvailabilityRepository(JsonDataContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public Task<Entities.room_availability?> GetByIdAsync(uint id)
    {
        return Task.FromResult(_context.room_availabilities            .FirstOrDefault(ra => ra.id == id));
    }

    public Task<IEnumerable<Entities.room_availability>> GetByRoomIdAsync(uint roomId)
    {
        IEnumerable<Entities.room_availability> results = _context.room_availabilities
            .Where(ra => ra.room_id == roomId)
            .ToList();

        return Task.FromResult(results);
    }

    public Task<IEnumerable<Entities.room_availability>> GetByScheduleRangeAsync(DateTime startSchedule, DateTime endSchedule)
    {
        IEnumerable<Entities.room_availability> results = _context.room_availabilities
            .Where(ra => ra.start_schedule >= startSchedule && ra.end_schedule <= endSchedule)
            .ToList();

        return Task.FromResult(results);
    }

    public Task<IEnumerable<Entities.room_availability>> GetOverlappingAvailabilitiesAsync(DateTime startDate, DateTime endDate)
    {
        IEnumerable<Entities.room_availability> results = _context.room_availabilities
            .Where(ra => ra.start_schedule < endDate && ra.end_schedule > startDate)
            .ToList();

        return Task.FromResult(results);
    }

    public Task<Entities.room?> GetRoomByAvailabilityIdAsync(uint availabilityId)
    {
        var availability = _context.room_availabilities
            .FirstOrDefault(ra => ra.id == availabilityId);

        var room = availability is null
            ? null
            : _context.rooms.FirstOrDefault(r => r.id == availability.room_id);

        return Task.FromResult(room);
    }

    public Task<IEnumerable<Entities.room_availability>> GetAllAvailabilitiesAsync()
    {
        IEnumerable<Entities.room_availability> results = _context.room_availabilities.ToList();
        return Task.FromResult(results);
    }
}



