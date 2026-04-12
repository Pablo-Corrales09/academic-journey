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
/// Implementation of ICustomerRepository
/// </summary>
public class CustomerRepository : ICustomerRepository
{
    private readonly Entities.HotelCargaContext _context;

    public CustomerRepository(Entities.HotelCargaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Entities.customer?> GetByIdAsync(uint id)
    {
        return await _context.customers.FirstOrDefaultAsync(c => c.id == id);
    }

    public async Task<Entities.customer?> GetByDocumentNumberAsync(string documentNumber)
    {
        return await _context.customers.FirstOrDefaultAsync(c => c.document_number == documentNumber);
    }

    public async Task<IEnumerable<Entities.customer>> SearchByNameAsync(string searchString)
    {
        return await _context.customers
            .Where(c => c.first_name!.Contains(searchString) || c.last_name!.Contains(searchString))
            .ToListAsync();
    }

    public async Task<Entities.customer?> GetByPhoneAsync(string phone)
    {
        return await _context.customers.FirstOrDefaultAsync(c => c.phone == phone);
    }

    public async Task<Entities.customer?> GetByDocumentNumberOrPhoneAsync(string documentNumber, string phone)
    {
        return await _context.customers
            .FirstOrDefaultAsync(c => c.document_number == documentNumber || c.phone == phone);
    }

    public async Task<Entities.customer?> GetByUserIdAsync(uint userId)
    {
        return await _context.customers.FirstOrDefaultAsync(c => c.user_id == userId);
    }

    public async Task<IEnumerable<uint>> GetBookingIdsByCustomerIdAsync(uint customerId)
    {
        return await _context.bookings
            .Where(b => b.customer_id == customerId)
            .Select(b => b.id)
            .ToListAsync();
    }

    public async Task<IEnumerable<uint>> GetBookingHistoryIdsByCustomerIdAsync(uint customerId)
    {
        return await _context.booking_histories
            .Where(bh => bh.customer_id == customerId)
            .Select(bh => bh.id)
            .ToListAsync();
    }

    public async Task<IEnumerable<uint>> GetWaitingQueueIdsByCustomerIdAsync(uint customerId)
    {
        return await _context.waiting_queues
            .Where(wq => wq.customer_id == customerId)
            .Select(wq => wq.id)
            .ToListAsync();
    }
}
