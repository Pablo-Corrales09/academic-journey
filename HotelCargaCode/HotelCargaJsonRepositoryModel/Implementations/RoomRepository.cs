using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelCargaJsonRepositoryModel.Interfaces;
using Entities = HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCargaJsonRepositoryModel.Implementations;

/// <summary>
/// Implementation of IRoomRepository
/// </summary>
public class RoomRepository : IRoomRepository
{
    private readonly JsonDataContext _context;

    public RoomRepository(JsonDataContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public Task<Entities.room?> GetByIdAsync(uint id)
    {
        return Task.FromResult(_context.rooms            .FirstOrDefault(r => r.id == id));
    }

    public Task<Entities.room?> GetByRoomNumberAsync(uint roomNumber)
    {
        return Task.FromResult(_context.rooms            .FirstOrDefault(r => r.room_number == roomNumber));
    }

    public Task<IEnumerable<Entities.room>> GetByNightlyRateRangeAsync(decimal minRate, decimal maxRate)
    {
        IEnumerable<Entities.room> results = _context.rooms
            .Where(r => r.nightly_rate >= minRate && r.nightly_rate <= maxRate)
            .ToList();
        return Task.FromResult(results);
    }

    public Task<IEnumerable<Entities.room>> GetByStatusIdAsync(byte statusId)
    {
        IEnumerable<Entities.room> results = _context.rooms
            .Where(r => r.status_id == statusId)
            .ToList();
        return Task.FromResult(results);
    }

    public Task<IEnumerable<Entities.room>> GetByCategoryIdAsync(byte categoryId)
    {
        IEnumerable<Entities.room> results = _context.rooms
            .Where(r => r.category_id == categoryId)
            .ToList();
        return Task.FromResult(results);
    }

    public Task<IEnumerable<Entities.room>> GetByFloorNumberAsync(byte floorNumber)
    {
        IEnumerable<Entities.room> results = _context.rooms
            .Where(r => r.floor_number == floorNumber)
            .ToList();
        return Task.FromResult(results);
    }

    public Task<IEnumerable<Entities.booking>> GetBookingsByRoomIdAsync(uint roomId)
    {
        IEnumerable<Entities.booking> results = _context.bookings
            .Where(b => b.room_id == roomId)
            .ToList();
        return Task.FromResult(results);
    }

    public Task<IEnumerable<Entities.booking_history>> GetBookingHistoriesByRoomIdAsync(uint roomId)
    {
        IEnumerable<Entities.booking_history> results = _context.booking_histories
            .Where(bh => bh.room_id == roomId)
            .ToList();
        return Task.FromResult(results);
    }

    public Task<IEnumerable<Entities.room_availability>> GetAvailabilitiesByRoomIdAsync(uint roomId)
    {
        IEnumerable<Entities.room_availability> results = _context.room_availabilities
            .Where(ra => ra.room_id == roomId)
            .ToList();
        return Task.FromResult(results);
    }
}



