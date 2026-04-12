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
/// Implementation of IWaitingQueueRepository
/// </summary>
public class WaitingQueueRepository : IWaitingQueueRepository
{
    private readonly Entities.HotelCargaContext _context;

    public WaitingQueueRepository(Entities.HotelCargaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Entities.waiting_queue?> GetByIdAsync(uint id)
    {
        return await _context.waiting_queues
            .Include(wq => wq.customer)
            .Include(wq => wq.room_category)
            .Include(wq => wq.status)
            .FirstOrDefaultAsync(wq => wq.id == id);
    }

    public async Task<IEnumerable<Entities.room_category>> GetCategoriesByCustomerIdAsync(uint customerId)
    {
        return await _context.waiting_queues
            .Where(wq => wq.customer_id == customerId)
            .Select(wq => wq.room_category)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.waiting_queue>> GetWithRelatedEntitiesByCustomerIdAsync(uint customerId)
    {
        return await _context.waiting_queues
            .Include(wq => wq.status)
            .Include(wq => wq.room_category)
            .Where(wq => wq.customer_id == customerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.room_category>> GetCategoriesByQueueStatusNameAsync(string statusName)
    {
        return await _context.waiting_queues
            .Where(wq => wq.status!.status_name == statusName)
            .Select(wq => wq.room_category)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.waiting_queue>> GetWithCustomerByQueueStatusNameAsync(string statusName)
    {
        return await _context.waiting_queues
            .Include(wq => wq.customer)
            .Where(wq => wq.status!.status_name == statusName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.waiting_queue>> GetAllQueuesAsync()
    {
        return await _context.waiting_queues.ToListAsync();
    }
}
