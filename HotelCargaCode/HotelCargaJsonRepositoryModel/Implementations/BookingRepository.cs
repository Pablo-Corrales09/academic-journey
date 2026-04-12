using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelCargaJsonRepositoryModel.Interfaces;
using Entities = HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCargaJsonRepositoryModel.Implementations;

/// <summary>
/// Implementation of IBookingRepository
/// </summary>
public class BookingRepository : IBookingRepository
{
    private readonly JsonDataContext _context;

    public BookingRepository(JsonDataContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public Task<string?> GetReserveNumberByIdAsync(uint id)
    {
        var booking = _context.bookings.FirstOrDefault(b => b.id == id);
        return Task.FromResult(booking?.reserve_number);
    }

    public Task<uint?> GetRoomIdByReserveNumberAsync(string reserveNumber)
    {
        var booking = _context.bookings.FirstOrDefault(b => b.reserve_number == reserveNumber);
        return Task.FromResult(booking?.room_id);
    }

    public Task<IEnumerable<uint>> GetBookingHistoryIdsByReserveNumberAsync(string reserveNumber)
    {
        var bookingIds = _context.bookings
            .Where(b => b.reserve_number == reserveNumber)
            .Select(b => b.id)
            .ToHashSet();

        IEnumerable<uint> results = _context.booking_histories
            .Where(bh => bookingIds.Contains(bh.booking_id))
            .Select(bh => bh.id)
            .ToList();

        return Task.FromResult(results);
    }

    public Task<IEnumerable<string>> GetOverlappingReserveNumbersAsync(DateTime startDate, DateTime endDate)
    {
        IEnumerable<string> results = _context.bookings
            .Where(b => b.check_in < endDate && b.check_out > startDate)
            .Select(b => b.reserve_number)
            .ToList();

        return Task.FromResult(results);
    }

    public Task<IEnumerable<string>> GetReserveNumbersByCheckInDateAsync(DateTime checkInDate)
    {
        IEnumerable<string> results = _context.bookings
            .Where(b => b.check_in.Date == checkInDate.Date)
            .Select(b => b.reserve_number)
            .ToList();

        return Task.FromResult(results);
    }

    public Task<decimal?> GetTotalPriceByReserveNumberAsync(string reserveNumber)
    {
        var booking = _context.bookings.FirstOrDefault(b => b.reserve_number == reserveNumber);
        return Task.FromResult(booking?.total_price);
    }

    public Task<IEnumerable<string>> GetReserveNumbersByStatusIdAsync(byte statusId)
    {
        IEnumerable<string> results = _context.bookings
            .Where(b => b.status_id == statusId)
            .Select(b => b.reserve_number)
            .ToList();

        return Task.FromResult(results);
    }

    public Task<IEnumerable<string>> GetReserveNumbersByCustomerIdAsync(uint customerId)
    {
        IEnumerable<string> results = _context.bookings
            .Where(b => b.customer_id == customerId)
            .Select(b => b.reserve_number)
            .ToList();

        return Task.FromResult(results);
    }
}



