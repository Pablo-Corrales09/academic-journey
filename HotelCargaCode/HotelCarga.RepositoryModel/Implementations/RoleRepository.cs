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
/// Implementation of IRoleRepository
/// </summary>
public class RoleRepository : IRoleRepository
{
    private readonly Entities.HotelCargaContext _context;

    public RoleRepository(Entities.HotelCargaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<string?> GetRoleNameByIdAsync(byte id)
    {
        var role = await _context.roles.FirstOrDefaultAsync(r => r.id == id);
        return role?.role_name;
    }

    public async Task<string?> GetDescriptionByRoleNameAsync(string roleName)
    {
        var role = await _context.roles.FirstOrDefaultAsync(r => r.role_name == roleName);
        return role?.description;
    }

    public async Task<IEnumerable<uint>> GetUserIdsByRoleNameAsync(string roleName)
    {
        return await _context.users
            .Where(u => u.role!.role_name == roleName)
            .Select(u => u.id)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.role>> GetAllRolesAsync()
    {
        return await _context.roles.ToListAsync();
    }
}
