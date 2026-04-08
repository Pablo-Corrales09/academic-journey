using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelCargaJsonRepositoryModel.Interfaces;
using Entities = HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCargaJsonRepositoryModel.Implementations;

/// <summary>
/// Implementation of IBookingStatusRepository
/// </summary>
public class BookingStatusRepository : IBookingStatusRepository
{
    private readonly JsonDataContext _context;

    public BookingStatusRepository(JsonDataContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public Task<Entities.booking_status?> GetByIdAsync(byte id)
    {
        return Task.FromResult(_context.booking_statuses.FirstOrDefault(bs => bs.id == id));
    }

    public Task<Entities.booking_status?> GetByStatusNameAsync(string statusName)
    {
        return Task.FromResult(_context.booking_statuses.FirstOrDefault(bs => bs.status_name == statusName));
    }

    public Task<IEnumerable<uint>> GetBookingHistoryIdsByStatusNameAsync(string statusName)
    {
        var statusId = _context.booking_statuses
            .FirstOrDefault(bs => bs.status_name == statusName)
            ?.id;

        if (statusId is null)
        {
            return Task.FromResult(Enumerable.Empty<uint>());
        }

        IEnumerable<uint> results = _context.booking_histories
            .Where(bh => bh.status_id == statusId)
            .Select(bh => bh.id)
            .ToList();

        return Task.FromResult(results);
    }

    public Task<string?> GetStatusNameByBookingIdAsync(uint bookingId)
    {
        var booking = _context.bookings
            .FirstOrDefault(b => b.id == bookingId);

        var status = booking is null
            ? null
            : _context.booking_statuses.FirstOrDefault(bs => bs.id == booking.status_id);

        return Task.FromResult(status?.status_name);
    }

    public Task<IEnumerable<Entities.booking_status>> GetAllStatusesAsync()
    {
        IEnumerable<Entities.booking_status> results = _context.booking_statuses.ToList();
        return Task.FromResult(results);
    }
}



