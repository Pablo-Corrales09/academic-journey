using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCarga.RepositoryModel.Interfaces;

/// <summary>
/// Repository interface for role entity
/// </summary>
public interface IRoleRepository
{
    /// <summary>
    /// Get a single role_name (string) by id (PK)
    /// </summary>
    Task<string?> GetRoleNameByIdAsync(byte id);

    /// <summary>
    /// Get a single description (string) given an exact role_name (string)
    /// </summary>
    Task<string?> GetDescriptionByRoleNameAsync(string roleName);

    /// <summary>
    /// Get a list of all related user IDs (uint) given an exact role_name (string)
    /// </summary>
    Task<IEnumerable<uint>> GetUserIdsByRoleNameAsync(string roleName);

    /// <summary>
    /// Get a list of all role entities (useful for dropdown menus)
    /// </summary>
    Task<IEnumerable<role>> GetAllRolesAsync();
}
