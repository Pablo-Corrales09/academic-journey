using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelCargaJsonRepositoryModel.Interfaces;
using Entities = HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCargaJsonRepositoryModel.Implementations;

/// <summary>
/// Implementation of IRoomStatusRepository
/// </summary>
public class RoomStatusRepository : IRoomStatusRepository
{
    private readonly JsonDataContext _context;

    public RoomStatusRepository(JsonDataContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public Task<Entities.room_status?> GetByIdAsync(byte id)
    {
        return Task.FromResult(_context.room_statuses.FirstOrDefault(rs => rs.id == id));
    }

    public Task<Entities.room_status?> GetByNameAsync(string name)
    {
        return Task.FromResult(_context.room_statuses.FirstOrDefault(rs => rs.status_name == name));
    }

    public Task<IEnumerable<Entities.room_status>> GetByDescriptionPartialAsync(string description)
    {
        IEnumerable<Entities.room_status> results = _context.room_statuses
            .Where(rs => rs.description!.Contains(description))
            .ToList();
        return Task.FromResult(results);
    }

    public Task<IEnumerable<Entities.room>> GetRoomsByStatusIdAsync(byte statusId)
    {
        IEnumerable<Entities.room> results = _context.rooms
            .Where(r => r.status_id == statusId)
            .ToList();
        return Task.FromResult(results);
    }

    public Task<IEnumerable<Entities.room_status>> GetAllStatusesAsync()
    {
        IEnumerable<Entities.room_status> results = _context.room_statuses.ToList();
        return Task.FromResult(results);
    }
}



