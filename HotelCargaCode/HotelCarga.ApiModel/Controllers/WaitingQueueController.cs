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
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.waiting_queues.FirstOrDefault(w => w.id == id));
        if (DbContext is null) return DbBackendMissing();
        var entity = await DbContext.Set<waiting_queue>().FindAsync(id);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll(bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.waiting_queues);
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<waiting_queue>().ToListAsync());
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] waiting_queue item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        await DbContext.Set<waiting_queue>().AddAsync(item);
        await DbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = item.id }, item);
    }

    [HttpPut("Update")]
    public async Task<IActionResult> Update([FromBody] waiting_queue item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        DbContext.Set<waiting_queue>().Update(item);
        await DbContext.SaveChangesAsync();
        return Ok(item);
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
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.waiting_queues);
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<waiting_queue>().ToListAsync());
    }
}
