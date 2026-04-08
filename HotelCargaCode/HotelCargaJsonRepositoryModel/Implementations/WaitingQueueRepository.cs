using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelCargaJsonRepositoryModel.Interfaces;
using Entities = HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCargaJsonRepositoryModel.Implementations;

/// <summary>
/// Implementation of IWaitingQueueRepository
/// </summary>
public class WaitingQueueRepository : IWaitingQueueRepository
{
    private readonly JsonDataContext _context;

    public WaitingQueueRepository(JsonDataContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public Task<Entities.waiting_queue?> GetByIdAsync(uint id)
    {
        return Task.FromResult(_context.waiting_queues            .FirstOrDefault(wq => wq.id == id));
    }

    public Task<IEnumerable<Entities.room_category>> GetCategoriesByCustomerIdAsync(uint customerId)
    {
        var categoryIds = _context.waiting_queues
            .Where(wq => wq.customer_id == customerId)
            .Select(wq => wq.room_category_id)
            .Distinct()
            .ToList();

        IEnumerable<Entities.room_category> results = _context.room_categories
            .Where(rc => categoryIds.Contains(rc.id))
            .ToList();
        return Task.FromResult(results);
    }

    public Task<IEnumerable<Entities.waiting_queue>> GetWithRelatedEntitiesByCustomerIdAsync(uint customerId)
    {
        var queues = _context.waiting_queues
            .Where(wq => wq.customer_id == customerId)
            .ToList();

        foreach (var queue in queues)
        {
            queue.customer = _context.customers.FirstOrDefault(c => c.id == queue.customer_id)!;
            queue.room_category = _context.room_categories.FirstOrDefault(rc => rc.id == queue.room_category_id)!;
            queue.status = _context.queue_statuses.FirstOrDefault(qs => qs.id == queue.status_id)!;
        }

        return Task.FromResult<IEnumerable<Entities.waiting_queue>>(queues);
    }

    public Task<IEnumerable<Entities.room_category>> GetCategoriesByQueueStatusNameAsync(string statusName)
    {
        var statusId = _context.queue_statuses
            .FirstOrDefault(qs => qs.status_name == statusName)
            ?.id;

        if (statusId is null)
        {
            return Task.FromResult(Enumerable.Empty<Entities.room_category>());
        }

        var categoryIds = _context.waiting_queues
            .Where(wq => wq.status_id == statusId)
            .Select(wq => wq.room_category_id)
            .Distinct()
            .ToList();

        IEnumerable<Entities.room_category> results = _context.room_categories
            .Where(rc => categoryIds.Contains(rc.id))
            .ToList();
        return Task.FromResult(results);
    }

    public Task<IEnumerable<Entities.waiting_queue>> GetWithCustomerByQueueStatusNameAsync(string statusName)
    {
        var statusId = _context.queue_statuses
            .FirstOrDefault(qs => qs.status_name == statusName)
            ?.id;

        if (statusId is null)
        {
            return Task.FromResult<IEnumerable<Entities.waiting_queue>>(Enumerable.Empty<Entities.waiting_queue>());
        }

        var queues = _context.waiting_queues
            .Where(wq => wq.status_id == statusId)
            .ToList();

        foreach (var queue in queues)
        {
            queue.customer = _context.customers.FirstOrDefault(c => c.id == queue.customer_id)!;
        }

        return Task.FromResult<IEnumerable<Entities.waiting_queue>>(queues);
    }

    public Task<IEnumerable<Entities.waiting_queue>> GetAllQueuesAsync()
    {
        IEnumerable<Entities.waiting_queue> results = _context.waiting_queues.ToList();
        return Task.FromResult(results);
    }
}



