using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCargaJsonRepositoryModel.Interfaces;

/// <summary>
/// Repository interface for waiting_queue entity
/// </summary>
public interface IWaitingQueueRepository
{
    /// <summary>
    /// Get a single waiting_queue entity by id (PK)
    /// </summary>
    Task<waiting_queue?> GetByIdAsync(uint id);

    /// <summary>
    /// Get a list of room_category entities related to an exact customer_id (uint)
    /// </summary>
    Task<IEnumerable<room_category>> GetCategoriesByCustomerIdAsync(uint customerId);

    /// <summary>
    /// Get a list of waiting_queue entities, including their related status and room_category entities, given an exact customer_id (uint)
    /// </summary>
    Task<IEnumerable<waiting_queue>> GetWithRelatedEntitiesByCustomerIdAsync(uint customerId);

    /// <summary>
    /// Get a list of all room_category entities filtering by the related queue_status entity's name property (string)
    /// </summary>
    Task<IEnumerable<room_category>> GetCategoriesByQueueStatusNameAsync(string statusName);

    /// <summary>
    /// Get a list of all waiting_queue entities, including their related customer entity, filtering by the related queue_status entity's name property (exact string match)
    /// </summary>
    Task<IEnumerable<waiting_queue>> GetWithCustomerByQueueStatusNameAsync(string statusName);

    /// <summary>
    /// Get a list of all waiting_queue entities (useful for dropdowns menus)
    /// </summary>
    Task<IEnumerable<waiting_queue>> GetAllQueuesAsync();
}

