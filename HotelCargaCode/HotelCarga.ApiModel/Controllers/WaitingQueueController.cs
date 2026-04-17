using System;
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
public class WaitingQueueController : BaseApiController
{
    public WaitingQueueController(HotelCargaContext? dbContext = null, JsonDataContext? jsonContext = null)
        : base(dbContext, jsonContext)
    {
    }

    [HttpGet("GetById")]
    public async Task<IActionResult> GetById(uint id, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            var entity = JsonContext!.waiting_queues.FirstOrDefault(w => w.id == id);
            if (entity is null) return NotFound();

            var customer = JsonContext.customers.FirstOrDefault(c => c.id == entity.customer_id);
            var category = JsonContext.room_categories.FirstOrDefault(c => c.id == entity.room_category_id);
            var status = JsonContext.queue_statuses.FirstOrDefault(s => s.id == entity.status_id);

            return Ok(new
            {
                entity.id,
                request_number = FormatRequestNumber(entity.id),
                entity.customer_id,
                customer_name = customer is null ? "Unknown" : $"{customer.first_name} {customer.last_name}".Trim(),
                entity.room_category_id,
                room_category_name = category?.category_name ?? "Unknown",
                entity.status_id,
                status_name = status?.status_name ?? $"STATUS {entity.status_id}",
                entity.requested_check_in,
                entity.check_out,
                entity.created_at,
                entity.updated_at
            });
        }

        if (DbContext is null) return DbBackendMissing();
        var item = await DbContext.Set<waiting_queue>()
            .Include(w => w.customer)
            .Include(w => w.room_category)
            .Include(w => w.status)
            .Where(w => w.id == id)
            .Select(w => new
            {
                w.id,
                request_number = FormatRequestNumber(w.id),
                w.customer_id,
                customer_name = ((w.customer.first_name ?? "") + " " + (w.customer.last_name ?? "")).Trim(),
                w.room_category_id,
                room_category_name = w.room_category.category_name,
                w.status_id,
                status_name = w.status.status_name,
                w.requested_check_in,
                w.check_out,
                w.created_at,
                w.updated_at
            })
            .FirstOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll(bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            var items = JsonContext!.waiting_queues
                .Select(w =>
                {
                    var customer = JsonContext.customers.FirstOrDefault(c => c.id == w.customer_id);
                    var category = JsonContext.room_categories.FirstOrDefault(c => c.id == w.room_category_id);
                    var status = JsonContext.queue_statuses.FirstOrDefault(s => s.id == w.status_id);

                    return new
                    {
                        w.id,
                        request_number = FormatRequestNumber(w.id),
                        w.customer_id,
                        customer_name = customer is null ? "Unknown" : $"{customer.first_name} {customer.last_name}".Trim(),
                        w.room_category_id,
                        room_category_name = category?.category_name ?? "Unknown",
                        w.status_id,
                        status_name = status?.status_name ?? $"STATUS {w.status_id}",
                        w.requested_check_in,
                        w.check_out,
                        w.created_at,
                        w.updated_at
                    };
                })
                .ToList();

            return Ok(items);
        }

        if (DbContext is null) return DbBackendMissing();

        var entries = await DbContext.Set<waiting_queue>()
            .Include(w => w.customer)
            .Include(w => w.room_category)
            .Include(w => w.status)
            .AsNoTracking()
            .Select(w => new
            {
                w.id,
                request_number = FormatRequestNumber(w.id),
                w.customer_id,
                customer_name = ((w.customer.first_name ?? "") + " " + (w.customer.last_name ?? "")).Trim(),
                w.room_category_id,
                room_category_name = w.room_category.category_name,
                w.status_id,
                status_name = w.status.status_name,
                w.requested_check_in,
                w.check_out,
                w.created_at,
                w.updated_at
            })
            .ToListAsync();

        return Ok(entries);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] waiting_queue item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();
        item.created_at ??= DateTime.UtcNow;
        item.updated_at = DateTime.UtcNow;

        await DbContext.Set<waiting_queue>().AddAsync(item);
        await DbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = item.id }, new
        {
            item.id,
            request_number = FormatRequestNumber(item.id),
            item.customer_id,
            item.room_category_id,
            item.status_id,
            item.requested_check_in,
            item.check_out,
            item.created_at,
            item.updated_at
        });
    }

    [HttpPut("Update")]
    public async Task<IActionResult> Update([FromBody] waiting_queue item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();
        item.updated_at = DateTime.UtcNow;

        DbContext.Set<waiting_queue>().Update(item);
        await DbContext.SaveChangesAsync();
        return Ok(new
        {
            item.id,
            request_number = FormatRequestNumber(item.id),
            item.customer_id,
            item.room_category_id,
            item.status_id,
            item.requested_check_in,
            item.check_out,
            item.created_at,
            item.updated_at
        });
    }

    private static string FormatRequestNumber(uint id)
    {
        var token = unchecked(id * 2654435761u);
        return $"RQ-{token:X8}";
    }

    [HttpDelete("Delete")]
    public async Task<IActionResult> Delete(uint id, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        var entity = await DbContext.Set<waiting_queue>().FindAsync(id);
        if (entity is null) return NotFound();

        DbContext.Set<waiting_queue>().Remove(entity);
        await DbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("GetCategoriesByCustomerId")]
    public async Task<IActionResult> GetCategoriesByCustomerId(uint customerId, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            var categoryIds = JsonContext!.waiting_queues.Where(w => w.customer_id == customerId).Select(w => w.room_category_id).Distinct();
            return Ok(JsonContext.room_categories.Where(c => categoryIds.Contains(c.id)));
        }

        if (DbContext is null) return DbBackendMissing();
        var ids = await DbContext.Set<waiting_queue>().Where(w => w.customer_id == customerId).Select(w => w.room_category_id).Distinct().ToListAsync();
        return Ok(await DbContext.Set<room_category>().Where(c => ids.Contains(c.id)).ToListAsync());
    }

    [HttpGet("GetWithRelatedEntitiesByCustomerId")]
    public async Task<IActionResult> GetWithRelatedEntitiesByCustomerId(uint customerId, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            var items = JsonContext!.waiting_queues.Where(w => w.customer_id == customerId).ToList();
            foreach (var item in items)
            {
                item.customer = JsonContext.customers.FirstOrDefault(c => c.id == item.customer_id)!;
                item.room_category = JsonContext.room_categories.FirstOrDefault(rc => rc.id == item.room_category_id)!;
                item.status = JsonContext.queue_statuses.FirstOrDefault(q => q.id == item.status_id)!;
            }
            return Ok(items);
        }

        if (DbContext is null) return DbBackendMissing();
        var entries = await DbContext.Set<waiting_queue>()
            .Include(w => w.customer)
            .Include(w => w.room_category)
            .Include(w => w.status)
            .Where(w => w.customer_id == customerId)
            .ToListAsync();
        return Ok(entries);
    }

    [HttpGet("GetCategoriesByQueueStatusName")]
    public async Task<IActionResult> GetCategoriesByQueueStatusName(string statusName, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            var status = JsonContext!.queue_statuses.FirstOrDefault(q => q.status_name == statusName);
            if (status is null) return Ok(Enumerable.Empty<room_category>());
            var categoryIds = JsonContext.waiting_queues.Where(w => w.status_id == status.id).Select(w => w.room_category_id).Distinct();
            return Ok(JsonContext.room_categories.Where(c => categoryIds.Contains(c.id)));
        }

        if (DbContext is null) return DbBackendMissing();
        var statusId = await DbContext.Set<queue_status>().Where(q => q.status_name == statusName).Select(q => (byte?)q.id).FirstOrDefaultAsync();
        if (statusId is null) return Ok(Enumerable.Empty<room_category>());
        var ids = await DbContext.Set<waiting_queue>().Where(w => w.status_id == statusId.Value).Select(w => w.room_category_id).Distinct().ToListAsync();
        return Ok(await DbContext.Set<room_category>().Where(c => ids.Contains(c.id)).ToListAsync());
    }

    [HttpGet("GetWithCustomerByQueueStatusName")]
    public async Task<IActionResult> GetWithCustomerByQueueStatusName(string statusName, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            var status = JsonContext!.queue_statuses.FirstOrDefault(q => q.status_name == statusName);
            if (status is null) return Ok(Enumerable.Empty<waiting_queue>());
            var items = JsonContext.waiting_queues.Where(w => w.status_id == status.id).ToList();
            foreach (var item in items)
            {
                item.customer = JsonContext.customers.FirstOrDefault(c => c.id == item.customer_id)!;
            }
            return Ok(items);
        }

        if (DbContext is null) return DbBackendMissing();
        var entries = await DbContext.Set<waiting_queue>()
            .Include(w => w.customer)
            .Where(w => w.status.status_name == statusName)
            .ToListAsync();
        return Ok(entries);
    }

    [HttpGet("GetAllQueues")]
    public async Task<IActionResult> GetAllQueues(bool useJson = false)
    {
        return await GetAll(useJson);
    }

    [HttpGet("GetFIFOQueueByRoomCategoryAndDateRange")]
    public async Task<IActionResult> GetFIFOQueueByRoomCategoryAndDateRange(byte roomCategoryId, DateTime checkInDate, DateTime checkOutDate, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            var entries = JsonContext!.waiting_queues
                .Where(w => w.room_category_id == roomCategoryId && w.status_id == 1 && w.requested_check_in >= checkInDate && (w.check_out == null || w.check_out <= checkOutDate))
                .OrderBy(w => w.created_at)
                .ToList();
            return Ok(entries);
        }

        if (DbContext is null) return DbBackendMissing();
        var queueEntries = await DbContext.Set<waiting_queue>()
            .Where(w => w.room_category_id == roomCategoryId && w.status_id == 1 && w.requested_check_in >= checkInDate && (w.check_out == null || w.check_out <= checkOutDate))
            .OrderBy(w => w.created_at)
            .Include(w => w.customer)
            .Include(w => w.room_category)
            .Include(w => w.status)
            .ToListAsync();
        return Ok(queueEntries);
    }

    [HttpGet("GetFIFOQueueByRoomCategory")]
    public async Task<IActionResult> GetFIFOQueueByRoomCategory(byte roomCategoryId, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            var entries = JsonContext!.waiting_queues
                .Where(w => w.room_category_id == roomCategoryId && w.status_id == 1)
                .OrderBy(w => w.created_at)
                .ToList();
            return Ok(entries);
        }

        if (DbContext is null) return DbBackendMissing();
        var queueEntries = await DbContext.Set<waiting_queue>()
            .Where(w => w.room_category_id == roomCategoryId && w.status_id == 1)
            .OrderBy(w => w.created_at)
            .Include(w => w.customer)
            .Include(w => w.room_category)
            .Include(w => w.status)
            .ToListAsync();
        return Ok(queueEntries);
    }

    [HttpGet("GetNotifiedQueueFIFO")]
    public async Task<IActionResult> GetNotifiedQueueFIFO(bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            var entries = JsonContext!.waiting_queues
                .Where(w => w.status_id == 2)
                .OrderBy(w => w.created_at)
                .ToList();
            return Ok(entries);
        }

        if (DbContext is null) return DbBackendMissing();
        var notifiedEntries = await DbContext.Set<waiting_queue>()
            .Where(w => w.status_id == 2)
            .OrderBy(w => w.created_at)
            .Include(w => w.customer)
            .Include(w => w.room_category)
            .Include(w => w.status)
            .ToListAsync();
        return Ok(notifiedEntries);
    }

    [HttpGet("CountPendingByRoomCategory")]
    public async Task<IActionResult> CountPendingByRoomCategory(byte roomCategoryId, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            var count = JsonContext!.waiting_queues.Count(w => w.room_category_id == roomCategoryId && w.status_id == 1);
            return Ok(new { count });
        }

        if (DbContext is null) return DbBackendMissing();
        var pendingCount = await DbContext.Set<waiting_queue>()
            .CountAsync(w => w.room_category_id == roomCategoryId && w.status_id == 1);
        return Ok(new { count = pendingCount });
    }
}
