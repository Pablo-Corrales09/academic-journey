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
/// Implementation of IBookingRepository
/// </summary>
public class BookingRepository : IBookingRepository
{
    private readonly Entities.HotelCargaContext _context;

    public BookingRepository(Entities.HotelCargaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<string?> GetReserveNumberByIdAsync(uint id)
    {
        var booking = await _context.bookings.FirstOrDefaultAsync(b => b.id == id);
        return booking?.reserve_number;
    }

    public async Task<uint?> GetRoomIdByReserveNumberAsync(string reserveNumber)
    {
        var booking = await _context.bookings.FirstOrDefaultAsync(b => b.reserve_number == reserveNumber);
        return booking?.room_id;
    }

    public async Task<IEnumerable<uint>> GetBookingHistoryIdsByReserveNumberAsync(string reserveNumber)
    {
        return await _context.booking_histories
            .Where(bh => bh.booking!.reserve_number == reserveNumber)
            .Select(bh => bh.id)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetOverlappingReserveNumbersAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.bookings
            .Where(b => b.check_in < endDate && b.check_out > startDate)
            .Where(b => b.reserve_number != null)
            .Select(b => b.reserve_number!)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetReserveNumbersByCheckInDateAsync(DateTime checkInDate)
    {
        return await _context.bookings
            .Where(b => b.check_in.Date == checkInDate.Date)
            .Where(b => b.reserve_number != null)
            .Select(b => b.reserve_number!)
            .ToListAsync();
    }

    public async Task<decimal?> GetTotalPriceByReserveNumberAsync(string reserveNumber)
    {
        var booking = await _context.bookings.FirstOrDefaultAsync(b => b.reserve_number == reserveNumber);
        return booking?.total_price;
    }

    public async Task<IEnumerable<string>> GetReserveNumbersByStatusIdAsync(byte statusId)
    {
        return await _context.bookings
            .Where(b => b.status_id == statusId)
            .Where(b => b.reserve_number != null)
            .Select(b => b.reserve_number!)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetReserveNumbersByCustomerIdAsync(uint customerId)
    {
        return await _context.bookings
            .Where(b => b.customer_id == customerId)
            .Where(b => b.reserve_number != null)
            .Select(b => b.reserve_number!)
            .ToListAsync();
    }
}
