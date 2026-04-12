using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCargaJsonRepositoryModel.Interfaces;

/// <summary>
/// Repository interface for queue_status entity
/// </summary>
public interface IQueueStatusRepository
{
    /// <summary>
    /// Get a single status_name by id (PK)
    /// </summary>
    Task<string?> GetStatusNameByIdAsync(byte id);

    /// <summary>
    /// Get a description (string) by exact status_name
    /// </summary>
    Task<string?> GetDescriptionByStatusNameAsync(string statusName);

    /// <summary>
    /// Query a list of all related waiting_queues IDs (uint) given an exact status_name (string)
    /// </summary>
    Task<IEnumerable<uint>> GetWaitingQueueIdsByStatusNameAsync(string statusName);

    /// <summary>
    /// Get a list of all queue_status entities (useful for dropdowns menus)
    /// </summary>
    Task<IEnumerable<queue_status>> GetAllStatusesAsync();
}

