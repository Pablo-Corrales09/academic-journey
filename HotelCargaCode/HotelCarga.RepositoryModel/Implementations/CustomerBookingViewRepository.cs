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
/// Implementation of ICustomerBookingViewRepository
/// </summary>
public class CustomerBookingViewRepository : ICustomerBookingViewRepository
{
    private readonly Entities.HotelCargaContext _context;

    public CustomerBookingViewRepository(Entities.HotelCargaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Entities.vw_customer_booking?> GetByReserveNumberAsync(string reserveNumber)
    {
        return await _context.vw_customer_bookings
            .FirstOrDefaultAsync(vcb => vcb.reserve_number == reserveNumber);
    }

    public async Task<IEnumerable<Entities.vw_customer_booking>> GetByCustomerIdAsync(uint customerId)
    {
        return await _context.vw_customer_bookings
            .Where(vcb => vcb.customer_id == customerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.vw_customer_booking>> GetByDocumentNumberAsync(string documentNumber)
    {
        return await _context.vw_customer_bookings
            .Where(vcb => vcb.document_number == documentNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.vw_customer_booking>> GetByBookingStatusAsync(string bookingStatus)
    {
        return await _context.vw_customer_bookings
            .Where(vcb => vcb.booking_status == bookingStatus)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.vw_customer_booking>> GetByCheckInDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.vw_customer_bookings
            .Where(vcb => vcb.check_in >= startDate && vcb.check_in <= endDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.vw_customer_booking>> GetByRoomNumberAndStatusAsync(uint roomNumber, string roomStatus)
    {
        return await _context.vw_customer_bookings
            .Where(vcb => vcb.room_number == roomNumber && vcb.room_status == roomStatus)
            .ToListAsync();
    }
}
