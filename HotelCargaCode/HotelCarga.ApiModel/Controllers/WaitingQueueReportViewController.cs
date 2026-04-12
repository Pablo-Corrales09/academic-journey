using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelCargaContext = HotelCarga.DbModel.HotelCargaContext;
using HotelCarga.HotelCarga.DbModel.Entities;
using HotelCarga.DbModel;
using HotelCargaJsonRepositoryModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelCarga.ApiModel.Controllers;

[Route("[controller]")]
public class WaitingQueueReportViewController : BaseApiController
{
    public WaitingQueueReportViewController(HotelCargaContext? dbContext = null, JsonDataContext? jsonContext = null)
        : base(dbContext, jsonContext)
    {
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll(bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(BuildWaitingQueueReportViews());
        if (DbContext is null) return DbBackendMissing();

        var result = await (from w in DbContext.Set<waiting_queue>()
                            join c in DbContext.Set<customer>() on w.customer_id equals c.id
                            join cat in DbContext.Set<room_category>() on w.room_category_id equals cat.id
                            join qs in DbContext.Set<queue_status>() on w.status_id equals qs.id
                            select new vw_waiting_queue_report
                            {
                                queue_id = w.id,
                                customer_id = c.id,
                                first_name = c.first_name,
                                last_name = c.last_name,
                                document_number = c.document_number,
                                requested_room_category = cat.category_name,
                                requested_check_in = w.requested_check_in,
                                check_out = w.check_out,
                                queue_status = qs.status_name,
                                current_status = qs.status_name,
                                created_at = w.created_at,
                                updated_at = w.updated_at
                            }).ToListAsync();
        return Ok(result);
    }

    [HttpGet("GetByQueueId")]
    public async Task<IActionResult> GetByQueueId(uint queueId, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(BuildWaitingQueueReportViews().FirstOrDefault(v => v.queue_id == queueId));
        if (DbContext is null) return DbBackendMissing();

        var result = await (from w in DbContext.Set<waiting_queue>()
                            join c in DbContext.Set<customer>() on w.customer_id equals c.id
                            join cat in DbContext.Set<room_category>() on w.room_category_id equals cat.id
                            join qs in DbContext.Set<queue_status>() on w.status_id equals qs.id
                            where w.id == queueId
                            select new vw_waiting_queue_report
                            {
                                queue_id = w.id,
                                customer_id = c.id,
                                first_name = c.first_name,
                                last_name = c.last_name,
                                document_number = c.document_number,
                                requested_room_category = cat.category_name,
                                requested_check_in = w.requested_check_in,
                                check_out = w.check_out,
                                queue_status = qs.status_name,
                                current_status = qs.status_name,
                                created_at = w.created_at,
                                updated_at = w.updated_at
                            }).FirstOrDefaultAsync();
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("GetByDocumentNumber")]
    public async Task<IActionResult> GetByDocumentNumber(string documentNumber, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(BuildWaitingQueueReportViews().Where(v => v.document_number == documentNumber));
        if (DbContext is null) return DbBackendMissing();

        var result = await (from w in DbContext.Set<waiting_queue>()
                            join c in DbContext.Set<customer>() on w.customer_id equals c.id
                            join cat in DbContext.Set<room_category>() on w.room_category_id equals cat.id
                            join qs in DbContext.Set<queue_status>() on w.status_id equals qs.id
                            where c.document_number == documentNumber
                            select new vw_waiting_queue_report
                            {
                                queue_id = w.id,
                                customer_id = c.id,
                                first_name = c.first_name,
                                last_name = c.last_name,
                                document_number = c.document_number,
                                requested_room_category = cat.category_name,
                                requested_check_in = w.requested_check_in,
                                check_out = w.check_out,
                                queue_status = qs.status_name,
                                current_status = qs.status_name,
                                created_at = w.created_at,
                                updated_at = w.updated_at
                            }).ToListAsync();
        return Ok(result);
    }

    [HttpGet("GetByCategoryAndStatus")]
    public async Task<IActionResult> GetByCategoryAndStatus(string requestedCategory, string queueStatus, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(BuildWaitingQueueReportViews().Where(v => v.requested_room_category == requestedCategory && v.queue_status == queueStatus));
        if (DbContext is null) return DbBackendMissing();

        var result = await (from w in DbContext.Set<waiting_queue>()
                            join c in DbContext.Set<customer>() on w.customer_id equals c.id
                            join cat in DbContext.Set<room_category>() on w.room_category_id equals cat.id
                            join qs in DbContext.Set<queue_status>() on w.status_id equals qs.id
                            where cat.category_name == requestedCategory && qs.status_name == queueStatus
                            select new vw_waiting_queue_report
                            {
                                queue_id = w.id,
                                customer_id = c.id,
                                first_name = c.first_name,
                                last_name = c.last_name,
                                document_number = c.document_number,
                                requested_room_category = cat.category_name,
                                requested_check_in = w.requested_check_in,
                                check_out = w.check_out,
                                queue_status = qs.status_name,
                                current_status = qs.status_name,
                                created_at = w.created_at,
                                updated_at = w.updated_at
                            }).ToListAsync();
        return Ok(result);
    }

    [HttpGet("GetByRequestedCheckInDateRange")]
    public async Task<IActionResult> GetByRequestedCheckInDateRange(DateTime startDate, DateTime endDate, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(BuildWaitingQueueReportViews().Where(v => v.requested_check_in >= startDate && v.requested_check_in <= endDate));
        if (DbContext is null) return DbBackendMissing();

        var result = await (from w in DbContext.Set<waiting_queue>()
                            join c in DbContext.Set<customer>() on w.customer_id equals c.id
                            join cat in DbContext.Set<room_category>() on w.room_category_id equals cat.id
                            join qs in DbContext.Set<queue_status>() on w.status_id equals qs.id
                            where w.requested_check_in >= startDate && w.requested_check_in <= endDate
                            select new vw_waiting_queue_report
                            {
                                queue_id = w.id,
                                customer_id = c.id,
                                first_name = c.first_name,
                                last_name = c.last_name,
                                document_number = c.document_number,
                                requested_room_category = cat.category_name,
                                requested_check_in = w.requested_check_in,
                                check_out = w.check_out,
                                queue_status = qs.status_name,
                                current_status = qs.status_name,
                                created_at = w.created_at,
                                updated_at = w.updated_at
                            }).ToListAsync();
        return Ok(result);
    }

    [HttpGet("SearchByName")]
    public async Task<IActionResult> SearchByName(string searchKeyword, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(BuildWaitingQueueReportViews().Where(v => v.first_name.Contains(searchKeyword) || v.last_name.Contains(searchKeyword)));
        if (DbContext is null) return DbBackendMissing();

        var result = await (from w in DbContext.Set<waiting_queue>()
                            join c in DbContext.Set<customer>() on w.customer_id equals c.id
                            join cat in DbContext.Set<room_category>() on w.room_category_id equals cat.id
                            join qs in DbContext.Set<queue_status>() on w.status_id equals qs.id
                            where c.first_name.Contains(searchKeyword) || c.last_name.Contains(searchKeyword)
                            select new vw_waiting_queue_report
                            {
                                queue_id = w.id,
                                customer_id = c.id,
                                first_name = c.first_name,
                                last_name = c.last_name,
                                document_number = c.document_number,
                                requested_room_category = cat.category_name,
                                requested_check_in = w.requested_check_in,
                                check_out = w.check_out,
                                queue_status = qs.status_name,
                                current_status = qs.status_name,
                                created_at = w.created_at,
                                updated_at = w.updated_at
                            }).ToListAsync();
        return Ok(result);
    }

    [HttpGet("GetByCurrentStatusOldestFirst")]
    public async Task<IActionResult> GetByCurrentStatusOldestFirst(string currentStatus, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(BuildWaitingQueueReportViews().Where(v => v.current_status == currentStatus).OrderBy(v => v.created_at));
        if (DbContext is null) return DbBackendMissing();

        var result = await (from w in DbContext.Set<waiting_queue>()
                            join c in DbContext.Set<customer>() on w.customer_id equals c.id
                            join cat in DbContext.Set<room_category>() on w.room_category_id equals cat.id
                            join qs in DbContext.Set<queue_status>() on w.status_id equals qs.id
                            where qs.status_name == currentStatus
                            orderby w.created_at
                            select new vw_waiting_queue_report
                            {
                                queue_id = w.id,
                                customer_id = c.id,
                                first_name = c.first_name,
                                last_name = c.last_name,
                                document_number = c.document_number,
                                requested_room_category = cat.category_name,
                                requested_check_in = w.requested_check_in,
                                check_out = w.check_out,
                                queue_status = qs.status_name,
                                current_status = qs.status_name,
                                created_at = w.created_at,
                                updated_at = w.updated_at
                            }).ToListAsync();
        return Ok(result);
    }

    private IEnumerable<vw_waiting_queue_report> BuildWaitingQueueReportViews()
    {
        return from w in JsonContext!.waiting_queues
               join c in JsonContext.customers on w.customer_id equals c.id
               join cat in JsonContext.room_categories on w.room_category_id equals cat.id
               join qs in JsonContext.queue_statuses on w.status_id equals qs.id
               select new vw_waiting_queue_report
               {
                   queue_id = w.id,
                   customer_id = c.id,
                   first_name = c.first_name,
                   last_name = c.last_name,
                   document_number = c.document_number,
                   requested_room_category = cat.category_name,
                   requested_check_in = w.requested_check_in,
                   check_out = w.check_out,
                   queue_status = qs.status_name,
                   current_status = qs.status_name,
                   created_at = w.created_at,
                   updated_at = w.updated_at
               };
    }
}
