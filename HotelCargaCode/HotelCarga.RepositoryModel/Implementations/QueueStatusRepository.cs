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
/// Implementation of IQueueStatusRepository
/// </summary>
public class QueueStatusRepository : IQueueStatusRepository
{
    private readonly Entities.HotelCargaContext _context;

    public QueueStatusRepository(Entities.HotelCargaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<string?> GetStatusNameByIdAsync(byte id)
    {
        var status = await _context.queue_statuses.FirstOrDefaultAsync(qs => qs.id == id);
        return status?.status_name;
    }

    public async Task<string?> GetDescriptionByStatusNameAsync(string statusName)
    {
        var status = await _context.queue_statuses.FirstOrDefaultAsync(qs => qs.status_name == statusName);
        return status?.description;
    }

    public async Task<IEnumerable<uint>> GetWaitingQueueIdsByStatusNameAsync(string statusName)
    {
        return await _context.waiting_queues
            .Where(wq => wq.status!.status_name == statusName)
            .Select(wq => wq.id)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.queue_status>> GetAllStatusesAsync()
    {
        return await _context.queue_statuses.ToListAsync();
    }
}
