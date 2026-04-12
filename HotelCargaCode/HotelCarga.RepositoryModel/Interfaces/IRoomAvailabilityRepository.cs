using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCarga.RepositoryModel.Interfaces;

/// <summary>
/// Repository interface for room_availability entity
/// </summary>
public interface IRoomAvailabilityRepository
{
    /// <summary>
    /// Query by id (PK)
    /// </summary>
    Task<room_availability?> GetByIdAsync(uint id);

    /// <summary>
    /// Query by exact room_id (uint)
    /// </summary>
    Task<IEnumerable<room_availability>> GetByRoomIdAsync(uint roomId);

    /// <summary>
    /// Query by schedule range (records that fall between a start_schedule and end_schedule)
    /// </summary>
    Task<IEnumerable<room_availability>> GetByScheduleRangeAsync(DateTime startSchedule, DateTime endSchedule);

    /// <summary>
    /// Query by overlapping dates (given a requested start and end date, find all availabilities that intersect to prevent double bookings)
    /// </summary>
    Task<IEnumerable<room_availability>> GetOverlappingAvailabilitiesAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Given a room_availability id, query the related room details
    /// </summary>
    Task<room?> GetRoomByAvailabilityIdAsync(uint availabilityId);

    /// <summary>
    /// Get a list of all room_availability entities (useful for dropdowns menus)
    /// </summary>
    Task<IEnumerable<room_availability>> GetAllAvailabilitiesAsync();
}
