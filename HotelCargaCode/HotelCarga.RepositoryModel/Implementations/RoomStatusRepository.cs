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
/// Implementation of IRoomStatusRepository
/// </summary>
public class RoomStatusRepository : IRoomStatusRepository
{
    private readonly Entities.HotelCargaContext _context;

    public RoomStatusRepository(Entities.HotelCargaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Entities.room_status?> GetByIdAsync(byte id)
    {
        return await _context.room_statuses.FirstOrDefaultAsync(rs => rs.id == id);
    }

    public async Task<Entities.room_status?> GetByNameAsync(string name)
    {
        return await _context.room_statuses.FirstOrDefaultAsync(rs => rs.status_name == name);
    }

    public async Task<IEnumerable<Entities.room_status>> GetByDescriptionPartialAsync(string description)
    {
        return await _context.room_statuses
            .Where(rs => rs.description!.Contains(description))
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.room>> GetRoomsByStatusIdAsync(byte statusId)
    {
        return await _context.rooms
            .Where(r => r.status_id == statusId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.room_status>> GetAllStatusesAsync()
    {
        return await _context.room_statuses.ToListAsync();
    }
}
