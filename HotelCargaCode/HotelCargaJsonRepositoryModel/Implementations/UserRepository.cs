using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelCargaJsonRepositoryModel.Interfaces;
using Entities = HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCargaJsonRepositoryModel.Implementations;

/// <summary>
/// Implementation of IUserRepository
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly JsonDataContext _context;

    public UserRepository(JsonDataContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public Task<string?> GetUsernameByIdAsync(uint id)
    {
        var user = _context.users.FirstOrDefault(u => u.id == id);
        return Task.FromResult(user?.username);
    }

    public Task<Entities.user?> GetUserWithRoleByUsernameAsync(string username)
    {
        var user = _context.users
            .FirstOrDefault(u => u.username == username);

        if (user is null)
        {
            return Task.FromResult<Entities.user?>(null);
        }

        user.role = _context.roles.FirstOrDefault(r => r.id == user.role_id)!;
        user.status = _context.user_statuses.FirstOrDefault(us => us.id == user.status_id)!;
        return Task.FromResult(user);
    }

    public Task<Entities.user?> GetUserWithRoleByEmailAsync(string email)
    {
        var user = _context.users
            .FirstOrDefault(u => u.email == email);

        if (user is null)
        {
            return Task.FromResult<Entities.user?>(null);
        }

        user.role = _context.roles.FirstOrDefault(r => r.id == user.role_id)!;
        user.status = _context.user_statuses.FirstOrDefault(us => us.id == user.status_id)!;
        return Task.FromResult(user);
    }

    public Task<bool> UsernameExistsAsync(string username)
    {
        return Task.FromResult(_context.users.Any(u => u.username == username));
    }

    public Task<bool> EmailExistsAsync(string email)
    {
        return Task.FromResult(_context.users.Any(u => u.email == email));
    }
}



