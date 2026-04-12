using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCargaJsonRepositoryModel.Interfaces;

/// <summary>
/// Repository interface for room_status entity
/// </summary>
public interface IRoomStatusRepository
{
    /// <summary>
    /// Query by id (PK)
    /// </summary>
    Task<room_status?> GetByIdAsync(byte id);

    /// <summary>
    /// Query by exact name (string)
    /// </summary>
    Task<room_status?> GetByNameAsync(string name);

    /// <summary>
    /// Query by partial description (string)
    /// </summary>
    Task<IEnumerable<room_status>> GetByDescriptionPartialAsync(string description);

    /// <summary>
    /// Given a room_status id query the related rooms
    /// </summary>
    Task<IEnumerable<room>> GetRoomsByStatusIdAsync(byte statusId);

    /// <summary>
    /// Get a list of all room_status entities (useful for dropdowns menus)
    /// </summary>
    Task<IEnumerable<room_status>> GetAllStatusesAsync();
}

