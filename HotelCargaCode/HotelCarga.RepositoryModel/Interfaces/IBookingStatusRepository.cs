using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCarga.RepositoryModel.Interfaces;

/// <summary>
/// Repository interface for booking_status entity
/// </summary>
public interface IBookingStatusRepository
{
    /// <summary>
    /// Query single booking_status by id (PK)
    /// </summary>
    Task<booking_status?> GetByIdAsync(byte id);

    /// <summary>
    /// Query single booking_status by exact status_name (string)
    /// </summary>
    Task<booking_status?> GetByStatusNameAsync(string statusName);

    /// <summary>
    /// Get a list of all booking_history id related to a specific status_name (string)
    /// </summary>
    Task<IEnumerable<uint>> GetBookingHistoryIdsByStatusNameAsync(string statusName);

    /// <summary>
    /// Get the status_name given a booking id
    /// </summary>
    Task<string?> GetStatusNameByBookingIdAsync(uint bookingId);

    /// <summary>
    /// Get a list of all booking_status entities (useful for dropdowns menus)
    /// </summary>
    Task<IEnumerable<booking_status>> GetAllStatusesAsync();
}
