using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelCargaJsonRepositoryModel.Interfaces;
using Entities = HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCargaJsonRepositoryModel.Implementations;

/// <summary>
/// Implementation of IUserStatusRepository
/// </summary>
public class UserStatusRepository : IUserStatusRepository
{
    private readonly JsonDataContext _context;

    public UserStatusRepository(JsonDataContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public Task<string?> GetStatusNameByIdAsync(byte id)
    {
        var status = _context.user_statuses.FirstOrDefault(us => us.id == id);
        return Task.FromResult(status?.status_name);
    }

    public Task<string?> GetDescriptionByStatusNameAsync(string statusName)
    {
        var status = _context.user_statuses.FirstOrDefault(us => us.status_name == statusName);
        return Task.FromResult(status?.description);
    }

    public Task<IEnumerable<uint>> GetUserIdsByStatusNameAsync(string statusName)
    {
        var statusId = _context.user_statuses
            .FirstOrDefault(us => us.status_name == statusName)
            ?.id;

        if (statusId is null)
        {
            return Task.FromResult(Enumerable.Empty<uint>());
        }

        IEnumerable<uint> results = _context.users
            .Where(u => u.status_id == statusId)
            .Select(u => u.id)
            .ToList();

        return Task.FromResult(results);
    }
}



