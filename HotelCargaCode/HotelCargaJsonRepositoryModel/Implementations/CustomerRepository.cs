using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelCargaJsonRepositoryModel.Interfaces;
using Entities = HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCargaJsonRepositoryModel.Implementations;

/// <summary>
/// Implementation of ICustomerRepository
/// </summary>
public class CustomerRepository : ICustomerRepository
{
    private readonly JsonDataContext _context;

    public CustomerRepository(JsonDataContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public Task<Entities.customer?> GetByIdAsync(uint id)
    {
        return Task.FromResult(_context.customers.FirstOrDefault(c => c.id == id));
    }

    public Task<Entities.customer?> GetByDocumentNumberAsync(string documentNumber)
    {
        return Task.FromResult(_context.customers.FirstOrDefault(c => c.document_number == documentNumber));
    }

    public Task<IEnumerable<Entities.customer>> SearchByNameAsync(string searchString)
    {
        IEnumerable<Entities.customer> results = _context.customers
            .Where(c => c.first_name!.Contains(searchString) || c.last_name!.Contains(searchString))
            .ToList();

        return Task.FromResult(results);
    }

    public Task<Entities.customer?> GetByPhoneAsync(string phone)
    {
        return Task.FromResult(_context.customers.FirstOrDefault(c => c.phone == phone));
    }

    public Task<Entities.customer?> GetByDocumentNumberOrPhoneAsync(string documentNumber, string phone)
    {
        return Task.FromResult(_context.customers
            .FirstOrDefault(c => c.document_number == documentNumber || c.phone == phone));
    }

    public Task<Entities.customer?> GetByUserIdAsync(uint userId)
    {
        return Task.FromResult(_context.customers.FirstOrDefault(c => c.user_id == userId));
    }

    public Task<IEnumerable<uint>> GetBookingIdsByCustomerIdAsync(uint customerId)
    {
        IEnumerable<uint> results = _context.bookings
            .Where(b => b.customer_id == customerId)
            .Select(b => b.id)
            .ToList();

        return Task.FromResult(results);
    }

    public Task<IEnumerable<uint>> GetBookingHistoryIdsByCustomerIdAsync(uint customerId)
    {
        IEnumerable<uint> results = _context.booking_histories
            .Where(bh => bh.customer_id == customerId)
            .Select(bh => bh.id)
            .ToList();

        return Task.FromResult(results);
    }

    public Task<IEnumerable<uint>> GetWaitingQueueIdsByCustomerIdAsync(uint customerId)
    {
        IEnumerable<uint> results = _context.waiting_queues
            .Where(wq => wq.customer_id == customerId)
            .Select(wq => wq.id)
            .ToList();

        return Task.FromResult(results);
    }
}



