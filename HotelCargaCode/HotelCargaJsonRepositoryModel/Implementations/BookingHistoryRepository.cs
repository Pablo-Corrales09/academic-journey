using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelCargaJsonRepositoryModel.Interfaces;
using Entities = HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCargaJsonRepositoryModel.Implementations;

/// <summary>
/// Implementation of IBookingHistoryRepository
/// </summary>
public class BookingHistoryRepository : IBookingHistoryRepository
{
    private readonly JsonDataContext _context;

    public BookingHistoryRepository(JsonDataContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public Task<Entities.booking_history?> GetByIdAsync(uint id)
    {
        return Task.FromResult(_context.booking_histories.FirstOrDefault(bh => bh.id == id));
    }

    public Task<IEnumerable<Entities.booking_history>> GetByBookingIdAsync(uint bookingId)
    {
        IEnumerable<Entities.booking_history> results = _context.booking_histories
            .Where(bh => bh.booking_id == bookingId)
            .OrderByDescending(bh => bh.created_at)
            .ToList();

        return Task.FromResult(results);
    }

    public Task<IEnumerable<Entities.booking_history>> GetByCustomerIdAsync(uint customerId)
    {
        IEnumerable<Entities.booking_history> results = _context.booking_histories
            .Where(bh => bh.customer_id == customerId)
            .OrderByDescending(bh => bh.created_at)
            .ToList();

        return Task.FromResult(results);
    }

    public Task<IEnumerable<Entities.booking_history>> GetByRoomIdAsync(uint roomId)
    {
        IEnumerable<Entities.booking_history> results = _context.booking_histories
            .Where(bh => bh.room_id == roomId)
            .OrderByDescending(bh => bh.created_at)
            .ToList();

        return Task.FromResult(results);
    }

    public Task<IEnumerable<Entities.booking_history>> GetByActionTypeAsync(string actionType)
    {
        IEnumerable<Entities.booking_history> results = _context.booking_histories
            .Where(bh => bh.action_type == actionType)
            .ToList();

        return Task.FromResult(results);
    }

    public Task<IEnumerable<Entities.booking_history>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        IEnumerable<Entities.booking_history> results = _context.booking_histories
            .Where(bh => bh.created_at >= startDate && bh.created_at <= endDate)
            .ToList();

        return Task.FromResult(results);
    }
}



