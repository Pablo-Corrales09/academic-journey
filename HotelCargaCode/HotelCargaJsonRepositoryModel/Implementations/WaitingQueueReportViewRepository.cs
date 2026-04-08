using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelCargaJsonRepositoryModel.Interfaces;
using Entities = HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCargaJsonRepositoryModel.Implementations;

/// <summary>
/// Implementation of IWaitingQueueReportViewRepository
/// </summary>
public class WaitingQueueReportViewRepository : IWaitingQueueReportViewRepository
{
    private readonly JsonDataContext _context;

    public WaitingQueueReportViewRepository(JsonDataContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    private IEnumerable<Entities.vw_waiting_queue_report> BuildViews()
    {
        return _context.waiting_queues.Select(queue =>
        {
            var customer = _context.customers.FirstOrDefault(c => c.id == queue.customer_id);
            var requestedCategory = _context.room_categories.FirstOrDefault(rc => rc.id == queue.room_category_id)?.category_name ?? string.Empty;
            var statusName = _context.queue_statuses.FirstOrDefault(qs => qs.id == queue.status_id)?.status_name ?? string.Empty;

            return new Entities.vw_waiting_queue_report
            {
                queue_id = queue.id,
                customer_id = customer?.id ?? 0,
                first_name = customer?.first_name ?? string.Empty,
                last_name = customer?.last_name ?? string.Empty,
                document_number = customer?.document_number ?? string.Empty,
                requested_room_category = requestedCategory,
                requested_check_in = queue.requested_check_in,
                check_out = queue.check_out,
                queue_status = statusName,
                current_status = statusName,
                created_at = queue.created_at,
                updated_at = queue.updated_at,
            };
        });
    }

    public Task<Entities.vw_waiting_queue_report?> GetByQueueIdAsync(uint queueId)
    {
        return Task.FromResult(BuildViews().FirstOrDefault(vwqr => vwqr.queue_id == queueId));
    }

    public Task<IEnumerable<Entities.vw_waiting_queue_report>> GetByDocumentNumberAsync(string documentNumber)
    {
        return Task.FromResult(BuildViews().Where(vwqr => vwqr.document_number == documentNumber).ToList().AsEnumerable());
    }

    public Task<IEnumerable<Entities.vw_waiting_queue_report>> GetByCategoryAndStatusAsync(string requestedCategory, string queueStatus)
    {
        return Task.FromResult(BuildViews()
            .Where(vwqr => vwqr.requested_room_category == requestedCategory && vwqr.queue_status == queueStatus)
            .ToList().AsEnumerable());
    }

    public Task<IEnumerable<Entities.vw_waiting_queue_report>> GetByRequestedCheckInDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return Task.FromResult(BuildViews()
            .Where(vwqr => vwqr.requested_check_in >= startDate && vwqr.requested_check_in <= endDate)
            .ToList().AsEnumerable());
    }

    public Task<IEnumerable<Entities.vw_waiting_queue_report>> SearchByNameAsync(string searchKeyword)
    {
        return Task.FromResult(BuildViews()
            .Where(vwqr => vwqr.first_name.Contains(searchKeyword) || vwqr.last_name.Contains(searchKeyword))
            .ToList().AsEnumerable());
    }

    public Task<IEnumerable<Entities.vw_waiting_queue_report>> GetByCurrentStatusOldestFirstAsync(string currentStatus)
    {
        return Task.FromResult(BuildViews()
            .Where(vwqr => vwqr.current_status == currentStatus)
            .OrderBy(vwqr => vwqr.created_at)
            .ToList().AsEnumerable());
    }
}



