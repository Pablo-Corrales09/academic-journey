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
/// Implementation of IWaitingQueueReportViewRepository
/// </summary>
public class WaitingQueueReportViewRepository : IWaitingQueueReportViewRepository
{
    private readonly Entities.HotelCargaContext _context;

    public WaitingQueueReportViewRepository(Entities.HotelCargaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Entities.vw_waiting_queue_report?> GetByQueueIdAsync(uint queueId)
    {
        return await _context.vw_waiting_queue_reports
            .FirstOrDefaultAsync(vwqr => vwqr.queue_id == queueId);
    }

    public async Task<IEnumerable<Entities.vw_waiting_queue_report>> GetByDocumentNumberAsync(string documentNumber)
    {
        return await _context.vw_waiting_queue_reports
            .Where(vwqr => vwqr.document_number == documentNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.vw_waiting_queue_report>> GetByCategoryAndStatusAsync(string requestedCategory, string queueStatus)
    {
        return await _context.vw_waiting_queue_reports
            .Where(vwqr => vwqr.requested_room_category == requestedCategory && vwqr.queue_status == queueStatus)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.vw_waiting_queue_report>> GetByRequestedCheckInDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.vw_waiting_queue_reports
            .Where(vwqr => vwqr.requested_check_in >= startDate && vwqr.requested_check_in <= endDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.vw_waiting_queue_report>> SearchByNameAsync(string searchKeyword)
    {
        return await _context.vw_waiting_queue_reports
            .Where(vwqr => vwqr.first_name!.Contains(searchKeyword) || vwqr.last_name!.Contains(searchKeyword))
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.vw_waiting_queue_report>> GetByCurrentStatusOldestFirstAsync(string currentStatus)
    {
        return await _context.vw_waiting_queue_reports
            .Where(vwqr => vwqr.current_status == currentStatus)
            .OrderBy(vwqr => vwqr.created_at)
            .ToListAsync();
    }
}
