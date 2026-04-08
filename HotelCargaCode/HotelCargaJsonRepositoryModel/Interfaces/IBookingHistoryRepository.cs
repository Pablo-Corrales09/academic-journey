using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCargaJsonRepositoryModel.Interfaces;

/// <summary>
/// Repository interface for booking_history entity
/// </summary>
public interface IBookingHistoryRepository
{
    /// <summary>
    /// Get single booking_history by id (PK)
    /// </summary>
    Task<booking_history?> GetByIdAsync(uint id);

    /// <summary>
    /// Get a list of all booking_history records related to a specific booking_id, ordered by created_at descending
    /// </summary>
    Task<IEnumerable<booking_history>> GetByBookingIdAsync(uint bookingId);

    /// <summary>
    /// Get a list of all booking_history records related to a specific customer_id, ordered by created_at descending
    /// </summary>
    Task<IEnumerable<booking_history>> GetByCustomerIdAsync(uint customerId);

    /// <summary>
    /// Get a list of all booking_history records related to a specific room_id, ordered by created_at descending
    /// </summary>
    Task<IEnumerable<booking_history>> GetByRoomIdAsync(uint roomId);

    /// <summary>
    /// Get a list of all booking_history records filtered by an exact action_type (string)
    /// </summary>
    Task<IEnumerable<booking_history>> GetByActionTypeAsync(string actionType);

    /// <summary>
    /// Get a list of all booking_history records where the created_at date falls between a given startDate and endDate
    /// </summary>
    Task<IEnumerable<booking_history>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}

