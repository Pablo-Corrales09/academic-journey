using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCarga.RepositoryModel.Interfaces;

/// <summary>
/// Repository interface for user entity
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Get a single username (string) by id (PK)
    /// </summary>
    Task<string?> GetUsernameByIdAsync(uint id);

    /// <summary>
    /// Get a single user entity, including its related role entity, by exact username (string)
    /// </summary>
    Task<user?> GetUserWithRoleByUsernameAsync(string username);

    /// <summary>
    /// Get a single user entity, including its related role entity, by exact email (string)
    /// </summary>
    Task<user?> GetUserWithRoleByEmailAsync(string email);

    /// <summary>
    /// Check if a username (string) exists (Returns boolean)
    /// </summary>
    Task<bool> UsernameExistsAsync(string username);

    /// <summary>
    /// Check if an email (string) exists (Returns boolean)
    /// </summary>
    Task<bool> EmailExistsAsync(string email);
}
