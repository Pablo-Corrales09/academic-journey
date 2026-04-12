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
/// Implementation of IUserStatusRepository
/// </summary>
public class UserStatusRepository : IUserStatusRepository
{
    private readonly Entities.HotelCargaContext _context;

    public UserStatusRepository(Entities.HotelCargaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<string?> GetStatusNameByIdAsync(byte id)
    {
        var status = await _context.user_statuses.FirstOrDefaultAsync(us => us.id == id);
        return status?.status_name;
    }

    public async Task<string?> GetDescriptionByStatusNameAsync(string statusName)
    {
        var status = await _context.user_statuses.FirstOrDefaultAsync(us => us.status_name == statusName);
        return status?.description;
    }

    public async Task<IEnumerable<uint>> GetUserIdsByStatusNameAsync(string statusName)
    {
        return await _context.users
            .Where(u => u.status!.status_name == statusName)
            .Select(u => u.id)
            .ToListAsync();
    }
}
