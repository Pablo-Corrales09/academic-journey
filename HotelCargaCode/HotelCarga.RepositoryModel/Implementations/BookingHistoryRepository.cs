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
/// Implementation of IBookingHistoryRepository
/// </summary>
public class BookingHistoryRepository : IBookingHistoryRepository
{
    private readonly Entities.HotelCargaContext _context;

    public BookingHistoryRepository(Entities.HotelCargaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Entities.booking_history?> GetByIdAsync(uint id)
    {
        return await _context.booking_histories.FirstOrDefaultAsync(bh => bh.id == id);
    }

    public async Task<IEnumerable<Entities.booking_history>> GetByBookingIdAsync(uint bookingId)
    {
        return await _context.booking_histories
            .Where(bh => bh.booking_id == bookingId)
            .OrderByDescending(bh => bh.created_at)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.booking_history>> GetByCustomerIdAsync(uint customerId)
    {
        return await _context.booking_histories
            .Where(bh => bh.customer_id == customerId)
            .OrderByDescending(bh => bh.created_at)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.booking_history>> GetByRoomIdAsync(uint roomId)
    {
        return await _context.booking_histories
            .Where(bh => bh.room_id == roomId)
            .OrderByDescending(bh => bh.created_at)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.booking_history>> GetByActionTypeAsync(string actionType)
    {
        return await _context.booking_histories
            .Where(bh => bh.action_type == actionType)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.booking_history>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.booking_histories
            .Where(bh => bh.created_at >= startDate && bh.created_at <= endDate)
            .ToListAsync();
    }
}
