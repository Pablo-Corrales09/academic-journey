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
/// Implementation of IRoomRepository
/// </summary>
public class RoomRepository : IRoomRepository
{
    private readonly Entities.HotelCargaContext _context;

    public RoomRepository(Entities.HotelCargaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Entities.room?> GetByIdAsync(uint id)
    {
        return await _context.rooms
            .Include(r => r.category)
            .Include(r => r.status)
            .FirstOrDefaultAsync(r => r.id == id);
    }

    public async Task<Entities.room?> GetByRoomNumberAsync(uint roomNumber)
    {
        return await _context.rooms
            .Include(r => r.category)
            .Include(r => r.status)
            .FirstOrDefaultAsync(r => r.room_number == roomNumber);
    }

    public async Task<IEnumerable<Entities.room>> GetByNightlyRateRangeAsync(decimal minRate, decimal maxRate)
    {
        return await _context.rooms
            .Where(r => r.nightly_rate >= minRate && r.nightly_rate <= maxRate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.room>> GetByStatusIdAsync(byte statusId)
    {
        return await _context.rooms
            .Where(r => r.status_id == statusId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.room>> GetByCategoryIdAsync(byte categoryId)
    {
        return await _context.rooms
            .Where(r => r.category_id == categoryId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.room>> GetByFloorNumberAsync(byte floorNumber)
    {
        return await _context.rooms
            .Where(r => r.floor_number == floorNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.booking>> GetBookingsByRoomIdAsync(uint roomId)
    {
        return await _context.bookings
            .Where(b => b.room_id == roomId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.booking_history>> GetBookingHistoriesByRoomIdAsync(uint roomId)
    {
        return await _context.booking_histories
            .Where(bh => bh.room_id == roomId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.room_availability>> GetAvailabilitiesByRoomIdAsync(uint roomId)
    {
        return await _context.room_availabilities
            .Where(ra => ra.room_id == roomId)
            .ToListAsync();
    }
}
