using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCarga.RepositoryModel.Interfaces;

/// <summary>
/// Repository interface for vw_customer_booking view
/// </summary>
public interface ICustomerBookingViewRepository
{
    /// <summary>
    /// Get a single vw_customer_bookings entity given an exact reserve_number (string)
    /// </summary>
    Task<vw_customer_booking?> GetByReserveNumberAsync(string reserveNumber);

    /// <summary>
    /// Get a list of vw_customer_booking entities given an exact customer_id (uint)
    /// </summary>
    Task<IEnumerable<vw_customer_booking>> GetByCustomerIdAsync(uint customerId);

    /// <summary>
    /// Get a list of vw_customer_booking entities given an exact document_number (string)
    /// </summary>
    Task<IEnumerable<vw_customer_booking>> GetByDocumentNumberAsync(string documentNumber);

    /// <summary>
    /// Get a list of vw_customer_booking entities filtering by an exact booking_status (string)
    /// </summary>
    Task<IEnumerable<vw_customer_booking>> GetByBookingStatusAsync(string bookingStatus);

    /// <summary>
    /// Get a list of vw_customer_booking entities where the check_in date falls within a specific date range
    /// </summary>
    Task<IEnumerable<vw_customer_booking>> GetByCheckInDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get a list of vw_customer_booking entities given an exact room_number (uint) and an exact room_status (string)
    /// </summary>
    Task<IEnumerable<vw_customer_booking>> GetByRoomNumberAndStatusAsync(uint roomNumber, string roomStatus);
}
