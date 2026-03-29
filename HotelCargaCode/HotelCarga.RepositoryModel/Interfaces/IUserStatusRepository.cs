using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCarga.RepositoryModel.Interfaces;

/// <summary>
/// Repository interface for user_status entity
/// </summary>
public interface IUserStatusRepository
{
    /// <summary>
    /// Get a single status_name by id (PK)
    /// </summary>
    Task<string?> GetStatusNameByIdAsync(byte id);

    /// <summary>
    /// Get a single description (string) by status_name (string)
    /// </summary>
    Task<string?> GetDescriptionByStatusNameAsync(string statusName);

    /// <summary>
    /// Get a list of all related users IDs (uint) given an exact status_name (string)
    /// </summary>
    Task<IEnumerable<uint>> GetUserIdsByStatusNameAsync(string statusName);
}
