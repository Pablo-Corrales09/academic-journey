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
public class QueueStatusController : BaseApiController
{
    public QueueStatusController(HotelCargaContext? dbContext = null, JsonDataContext? jsonContext = null)
        : base(dbContext, jsonContext)
    {
    }

    [HttpGet("GetById")]
    public async Task<IActionResult> GetById(byte id, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.queue_statuses.FirstOrDefault(s => s.id == id));
        if (DbContext is null) return DbBackendMissing();
        var entity = await DbContext.Set<queue_status>().FindAsync(id);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll(bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.queue_statuses);
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<queue_status>().ToListAsync());
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] queue_status item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        await DbContext.Set<queue_status>().AddAsync(item);
        await DbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = item.id }, item);
    }

    [HttpPut("Update")]
    public async Task<IActionResult> Update([FromBody] queue_status item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        DbContext.Set<queue_status>().Update(item);
        await DbContext.SaveChangesAsync();
        return Ok(item);
    }

    [HttpDelete("Delete")]
    public async Task<IActionResult> Delete(byte id, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        var entity = await DbContext.Set<queue_status>().FindAsync(id);
        if (entity is null) return NotFound();

        DbContext.Set<queue_status>().Remove(entity);
        await DbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("GetStatusNameById")]
    public async Task<IActionResult> GetStatusNameById(byte id, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.queue_statuses.FirstOrDefault(s => s.id == id)?.status_name);
        if (DbContext is null) return DbBackendMissing();
        var result = await DbContext.Set<queue_status>().Where(s => s.id == id).Select(s => s.status_name).FirstOrDefaultAsync();
        return Ok(result);
    }

    [HttpGet("GetDescriptionByStatusName")]
    public async Task<IActionResult> GetDescriptionByStatusName(string statusName, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.queue_statuses.FirstOrDefault(s => s.status_name == statusName)?.description);
        if (DbContext is null) return DbBackendMissing();
        var result = await DbContext.Set<queue_status>().Where(s => s.status_name == statusName).Select(s => s.description).FirstOrDefaultAsync();
        return Ok(result);
    }

    [HttpGet("GetWaitingQueueIdsByStatusName")]
    public async Task<IActionResult> GetWaitingQueueIdsByStatusName(string statusName, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            var status = JsonContext!.queue_statuses.FirstOrDefault(s => s.status_name == statusName);
            if (status is null) return Ok(Enumerable.Empty<uint>());
            return Ok(JsonContext.waiting_queues.Where(w => w.status_id == status.id).Select(w => w.id));
        }

        if (DbContext is null) return DbBackendMissing();
        var statusId = await DbContext.Set<queue_status>().Where(s => s.status_name == statusName).Select(s => (byte?)s.id).FirstOrDefaultAsync();
        if (statusId is null) return Ok(Enumerable.Empty<uint>());
        return Ok(await DbContext.Set<waiting_queue>().Where(w => w.status_id == statusId.Value).Select(w => w.id).ToListAsync());
    }

    [HttpGet("GetAllStatuses")]
    public async Task<IActionResult> GetAllStatuses(bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.queue_statuses);
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<queue_status>().ToListAsync());
    }
}
