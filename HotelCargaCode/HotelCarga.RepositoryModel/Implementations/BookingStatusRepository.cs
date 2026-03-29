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
/// Implementation of IBookingStatusRepository
/// </summary>
public class BookingStatusRepository : IBookingStatusRepository
{
    private readonly Entities.HotelCargaContext _context;

    public BookingStatusRepository(Entities.HotelCargaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Entities.booking_status?> GetByIdAsync(byte id)
    {
        return await _context.booking_statuses.FirstOrDefaultAsync(bs => bs.id == id);
    }

    public async Task<Entities.booking_status?> GetByStatusNameAsync(string statusName)
    {
        return await _context.booking_statuses.FirstOrDefaultAsync(bs => bs.status_name == statusName);
    }

    public async Task<IEnumerable<uint>> GetBookingHistoryIdsByStatusNameAsync(string statusName)
    {
        return await _context.booking_histories
            .Where(bh => bh.status!.status_name == statusName)
            .Select(bh => bh.id)
            .ToListAsync();
    }

    public async Task<string?> GetStatusNameByBookingIdAsync(uint bookingId)
    {
        var booking = await _context.bookings
            .Include(b => b.status)
            .FirstOrDefaultAsync(b => b.id == bookingId);
        
        return booking?.status?.status_name;
    }

    public async Task<IEnumerable<Entities.booking_status>> GetAllStatusesAsync()
    {
        return await _context.booking_statuses.ToListAsync();
    }
}
