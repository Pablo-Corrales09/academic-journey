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
public class RoomStatusController : BaseApiController
{
    public RoomStatusController(HotelCargaContext? dbContext = null, JsonDataContext? jsonContext = null)
        : base(dbContext, jsonContext)
    {
    }

    [HttpGet("GetById")]
    public async Task<IActionResult> GetById(byte id, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.room_statuses.FirstOrDefault(s => s.id == id));
        if (DbContext is null) return DbBackendMissing();
        var entity = await DbContext.Set<room_status>().FindAsync(id);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll(bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.room_statuses);
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<room_status>().ToListAsync());
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] room_status item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        await DbContext.Set<room_status>().AddAsync(item);
        await DbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = item.id }, item);
    }

    [HttpPut("Update")]
    public async Task<IActionResult> Update([FromBody] room_status item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        DbContext.Set<room_status>().Update(item);
        await DbContext.SaveChangesAsync();
        return Ok(item);
    }

    [HttpDelete("Delete")]
    public async Task<IActionResult> Delete(byte id, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        var entity = await DbContext.Set<room_status>().FindAsync(id);
        if (entity is null) return NotFound();

        DbContext.Set<room_status>().Remove(entity);
        await DbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("GetByName")]
    public async Task<IActionResult> GetByName(string name, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.room_statuses.FirstOrDefault(s => s.status_name == name));
        if (DbContext is null) return DbBackendMissing();
        var status = await DbContext.Set<room_status>().FirstOrDefaultAsync(s => s.status_name == name);
        return status is null ? NotFound() : Ok(status);
    }

    [HttpGet("GetByDescriptionPartial")]
    public async Task<IActionResult> GetByDescriptionPartial(string description, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.room_statuses.Where(s => s.description.Contains(description)));
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<room_status>().Where(s => s.description.Contains(description)).ToListAsync());
    }

    [HttpGet("GetRoomsByStatusId")]
    public async Task<IActionResult> GetRoomsByStatusId(byte statusId, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.rooms.Where(r => r.status_id == statusId));
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<room>().Where(r => r.status_id == statusId).ToListAsync());
    }

    [HttpGet("GetAllStatuses")]
    public async Task<IActionResult> GetAllStatuses(bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.room_statuses);
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<room_status>().ToListAsync());
    }
}
