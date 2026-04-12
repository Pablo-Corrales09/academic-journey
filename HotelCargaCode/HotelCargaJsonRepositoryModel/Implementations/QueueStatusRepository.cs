using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelCargaJsonRepositoryModel.Interfaces;
using Entities = HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCargaJsonRepositoryModel.Implementations;

/// <summary>
/// Implementation of IQueueStatusRepository
/// </summary>
public class QueueStatusRepository : IQueueStatusRepository
{
    private readonly JsonDataContext _context;

    public QueueStatusRepository(JsonDataContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public Task<string?> GetStatusNameByIdAsync(byte id)
    {
        var status = _context.queue_statuses.FirstOrDefault(qs => qs.id == id);
        return Task.FromResult(status?.status_name);
    }

    public Task<string?> GetDescriptionByStatusNameAsync(string statusName)
    {
        var status = _context.queue_statuses.FirstOrDefault(qs => qs.status_name == statusName);
        return Task.FromResult(status?.description);
    }

    public Task<IEnumerable<uint>> GetWaitingQueueIdsByStatusNameAsync(string statusName)
    {
        var statusId = _context.queue_statuses
            .FirstOrDefault(qs => qs.status_name == statusName)
            ?.id;

        if (statusId is null)
        {
            return Task.FromResult(Enumerable.Empty<uint>());
        }

        IEnumerable<uint> results = _context.waiting_queues
            .Where(wq => wq.status_id == statusId)
            .Select(wq => wq.id)
            .ToList();

        return Task.FromResult(results);
    }

    public Task<IEnumerable<Entities.queue_status>> GetAllStatusesAsync()
    {
        IEnumerable<Entities.queue_status> results = _context.queue_statuses.ToList();
        return Task.FromResult(results);
    }
}



