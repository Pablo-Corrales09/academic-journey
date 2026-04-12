using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelCargaJsonRepositoryModel.Interfaces;
using Entities = HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCargaJsonRepositoryModel.Implementations;

/// <summary>
/// Implementation of IRoleRepository
/// </summary>
public class RoleRepository : IRoleRepository
{
    private readonly JsonDataContext _context;

    public RoleRepository(JsonDataContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public Task<string?> GetRoleNameByIdAsync(byte id)
    {
        var role = _context.roles.FirstOrDefault(r => r.id == id);
        return Task.FromResult(role?.role_name);
    }

    public Task<string?> GetDescriptionByRoleNameAsync(string roleName)
    {
        var role = _context.roles.FirstOrDefault(r => r.role_name == roleName);
        return Task.FromResult(role?.description);
    }

    public Task<IEnumerable<uint>> GetUserIdsByRoleNameAsync(string roleName)
    {
        var roleId = _context.roles
            .FirstOrDefault(r => r.role_name == roleName)
            ?.id;

        if (roleId is null)
        {
            return Task.FromResult(Enumerable.Empty<uint>());
        }

        IEnumerable<uint> results = _context.users
            .Where(u => u.role_id == roleId)
            .Select(u => u.id)
            .ToList();

        return Task.FromResult(results);
    }

    public Task<IEnumerable<Entities.role>> GetAllRolesAsync()
    {
        IEnumerable<Entities.role> results = _context.roles.ToList();
        return Task.FromResult(results);
    }
}



