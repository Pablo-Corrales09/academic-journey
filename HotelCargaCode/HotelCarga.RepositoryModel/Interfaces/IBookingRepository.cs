using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCarga.RepositoryModel.Interfaces;

/// <summary>
/// Repository interface for booking entity
/// </summary>
public interface IBookingRepository
{
    /// <summary>
    /// Get the reserve_number (string) by id (PK)
    /// </summary>
    Task<string?> GetReserveNumberByIdAsync(uint id);

    /// <summary>
    /// Get the room_id (uint) given an exact reserve_number (string)
    /// </summary>
    Task<uint?> GetRoomIdByReserveNumberAsync(string reserveNumber);

    /// <summary>
    /// Get a list of all booking_history IDs (uint) related to a specific reserve_number
    /// </summary>
    Task<IEnumerable<uint>> GetBookingHistoryIdsByReserveNumberAsync(string reserveNumber);

    /// <summary>
    /// Get a list of reserve_number (strings) for bookings that overlap with a given startDate and endDate
    /// </summary>
    Task<IEnumerable<string>> GetOverlappingReserveNumbersAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get a list of reserve_number (strings) given an exact check_in Date
    /// </summary>
    Task<IEnumerable<string>> GetReserveNumbersByCheckInDateAsync(DateTime checkInDate);

    /// <summary>
    /// Get the total_price (decimal) given an exact reserve_number
    /// </summary>
    Task<decimal?> GetTotalPriceByReserveNumberAsync(string reserveNumber);

    /// <summary>
    /// Get a list of reserve_number (strings) related to a specific status_id
    /// </summary>
    Task<IEnumerable<string>> GetReserveNumbersByStatusIdAsync(byte statusId);

    /// <summary>
    /// Get a list of reserve_number (strings) related to a specific customer_id
    /// </summary>
    Task<IEnumerable<string>> GetReserveNumbersByCustomerIdAsync(uint customerId);
}
