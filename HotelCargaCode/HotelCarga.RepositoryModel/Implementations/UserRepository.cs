using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelCarga.RepositoryModel.Interfaces;
using Microsoft.EntityFrameworkCore;
using Entities = HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCarga.RepositoryModel.Implementations;

/// <summary>
/// Implementation of IUserRepository
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly Entities.HotelCargaContext _context;

    public UserRepository(Entities.HotelCargaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<string?> GetUsernameByIdAsync(uint id)
    {
        var user = await _context.users.FirstOrDefaultAsync(u => u.id == id);
        return user?.username;
    }

    public async Task<Entities.user?> GetUserWithRoleByUsernameAsync(string username)
    {
        return await _context.users
            .Include(u => u.role)
            .FirstOrDefaultAsync(u => u.username == username);
    }

    public async Task<Entities.user?> GetUserWithRoleByEmailAsync(string email)
    {
        return await _context.users
            .Include(u => u.role)
            .FirstOrDefaultAsync(u => u.email == email);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _context.users.AnyAsync(u => u.username == username);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.users.AnyAsync(u => u.email == email);
    }
}
