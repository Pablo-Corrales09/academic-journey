using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCarga.RepositoryModel.Interfaces;

/// <summary>
/// Repository interface for room entity
/// </summary>
public interface IRoomRepository
{
    /// <summary>
    /// Query by id (PK)
    /// </summary>
    Task<room?> GetByIdAsync(uint id);

    /// <summary>
    /// Query by exact room_number
    /// </summary>
    Task<room?> GetByRoomNumberAsync(uint roomNumber);

    /// <summary>
    /// Query by nightly_rate range (min_rate and max_rate)
    /// </summary>
    Task<IEnumerable<room>> GetByNightlyRateRangeAsync(decimal minRate, decimal maxRate);

    /// <summary>
    /// Given a status_id query all rooms related to the status_id
    /// </summary>
    Task<IEnumerable<room>> GetByStatusIdAsync(byte statusId);

    /// <summary>
    /// Given a category_id query all rooms related to the category_id
    /// </summary>
    Task<IEnumerable<room>> GetByCategoryIdAsync(byte categoryId);

    /// <summary>
    /// Query by exact floor_number
    /// </summary>
    Task<IEnumerable<room>> GetByFloorNumberAsync(byte floorNumber);

    /// <summary>
    /// Given a room id Query all its related bookings
    /// </summary>
    Task<IEnumerable<booking>> GetBookingsByRoomIdAsync(uint roomId);

    /// <summary>
    /// Given a room id Query all its booking_histories
    /// </summary>
    Task<IEnumerable<booking_history>> GetBookingHistoriesByRoomIdAsync(uint roomId);

    /// <summary>
    /// Given a room id Query all its related room_availabilities
    /// </summary>
    Task<IEnumerable<room_availability>> GetAvailabilitiesByRoomIdAsync(uint roomId);
}
