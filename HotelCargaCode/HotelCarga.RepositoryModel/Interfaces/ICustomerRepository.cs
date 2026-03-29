using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCarga.RepositoryModel.Interfaces;

/// <summary>
/// Repository interface for customer entity
/// </summary>
public interface ICustomerRepository
{
    /// <summary>
    /// Get a single customer entity by id (PK)
    /// </summary>
    Task<customer?> GetByIdAsync(uint id);

    /// <summary>
    /// Get a single customer entity by exact document_number (string)
    /// </summary>
    Task<customer?> GetByDocumentNumberAsync(string documentNumber);

    /// <summary>
    /// Get a list of customer entities where a given search string is partially contained (LIKE) in either first_name OR last_name
    /// </summary>
    Task<IEnumerable<customer>> SearchByNameAsync(string searchString);

    /// <summary>
    /// Get a single customer entity by exact phone (string)
    /// </summary>
    Task<customer?> GetByPhoneAsync(string phone);

    /// <summary>
    /// Get a single customer entity matching EITHER an exact document_number (string) OR an exact phone (string)
    /// </summary>
    Task<customer?> GetByDocumentNumberOrPhoneAsync(string documentNumber, string phone);

    /// <summary>
    /// Get a single customer entity by exact user_id (uint)
    /// </summary>
    Task<customer?> GetByUserIdAsync(uint userId);

    /// <summary>
    /// Get a list of related booking IDs (uint) given to a customer_id (uint)
    /// </summary>
    Task<IEnumerable<uint>> GetBookingIdsByCustomerIdAsync(uint customerId);

    /// <summary>
    /// Get a list of related booking_history IDs (uint) given a customer_id (uint)
    /// </summary>
    Task<IEnumerable<uint>> GetBookingHistoryIdsByCustomerIdAsync(uint customerId);

    /// <summary>
    /// Get a list of related waiting_queue IDs (uint) given a customer_id (uint)
    /// </summary>
    Task<IEnumerable<uint>> GetWaitingQueueIdsByCustomerIdAsync(uint customerId);
}
