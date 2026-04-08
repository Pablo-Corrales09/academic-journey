using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelCargaJsonRepositoryModel.Interfaces;
using Entities = HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCargaJsonRepositoryModel.Implementations;

/// <summary>
/// Implementation of ICustomerBookingViewRepository
/// </summary>
public class CustomerBookingViewRepository : ICustomerBookingViewRepository
{
    private readonly JsonDataContext _context;

    public CustomerBookingViewRepository(JsonDataContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    private IEnumerable<Entities.vw_customer_booking> BuildViews()
    {
        return _context.bookings.Select(booking =>
        {
            var customer = _context.customers.FirstOrDefault(c => c.id == booking.customer_id);
            var user = customer is null ? null : _context.users.FirstOrDefault(u => u.id == customer.user_id);
            var room = _context.rooms.FirstOrDefault(r => r.id == booking.room_id);
            var bookingStatus = _context.booking_statuses.FirstOrDefault(bs => bs.id == booking.status_id);
            var roomCategory = room is null ? null : _context.room_categories.FirstOrDefault(rc => rc.id == room.category_id);
            var roomStatus = room is null ? null : _context.room_statuses.FirstOrDefault(rs => rs.id == room.status_id);

            return new Entities.vw_customer_booking
            {
                customer_id = customer?.id ?? 0,
                first_name = customer?.first_name ?? string.Empty,
                last_name = customer?.last_name ?? string.Empty,
                document_number = customer?.document_number ?? string.Empty,
                phone = customer?.phone ?? string.Empty,
                email = user?.email ?? string.Empty,
                address = customer?.address ?? string.Empty,
                city = customer?.city ?? string.Empty,
                country = customer?.country ?? string.Empty,
                booking_id = booking.id,
                reserve_number = booking.reserve_number,
                check_in = booking.check_in,
                check_out = booking.check_out,
                nightly_rate = booking.nightly_rate,
                total_price = booking.total_price,
                booking_status = bookingStatus?.status_name ?? string.Empty,
                room_number = room?.room_number,
                floor_number = room?.floor_number,
                room_category = roomCategory?.category_name ?? string.Empty,
                room_status = roomStatus?.status_name ?? string.Empty,
                booking_created_at = booking.created_at,
                booking_updated_at = booking.updated_at,
            };
        });
    }

    public Task<Entities.vw_customer_booking?> GetByReserveNumberAsync(string reserveNumber)
    {
        return Task.FromResult(BuildViews().FirstOrDefault(vcb => vcb.reserve_number == reserveNumber));
    }

    public Task<IEnumerable<Entities.vw_customer_booking>> GetByCustomerIdAsync(uint customerId)
    {
        return Task.FromResult(BuildViews().Where(vcb => vcb.customer_id == customerId).ToList().AsEnumerable());
    }

    public Task<IEnumerable<Entities.vw_customer_booking>> GetByDocumentNumberAsync(string documentNumber)
    {
        return Task.FromResult(BuildViews().Where(vcb => vcb.document_number == documentNumber).ToList().AsEnumerable());
    }

    public Task<IEnumerable<Entities.vw_customer_booking>> GetByBookingStatusAsync(string bookingStatus)
    {
        return Task.FromResult(BuildViews().Where(vcb => vcb.booking_status == bookingStatus).ToList().AsEnumerable());
    }

    public Task<IEnumerable<Entities.vw_customer_booking>> GetByCheckInDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return Task.FromResult(BuildViews().Where(vcb => vcb.check_in >= startDate && vcb.check_in <= endDate).ToList().AsEnumerable());
    }

    public Task<IEnumerable<Entities.vw_customer_booking>> GetByRoomNumberAndStatusAsync(uint roomNumber, string roomStatus)
    {
        return Task.FromResult(BuildViews().Where(vcb => vcb.room_number == roomNumber && vcb.room_status == roomStatus).ToList().AsEnumerable());
    }
}



