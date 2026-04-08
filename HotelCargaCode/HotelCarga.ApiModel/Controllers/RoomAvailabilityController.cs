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
public class RoomAvailabilityController : BaseApiController
{
    public RoomAvailabilityController(HotelCargaContext? dbContext = null, JsonDataContext? jsonContext = null)
        : base(dbContext, jsonContext)
    {
    }

    [HttpGet("GetById")]
    public async Task<IActionResult> GetById(uint id, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.room_availabilities.FirstOrDefault(a => a.id == id));
        if (DbContext is null) return DbBackendMissing();
        var entity = await DbContext.Set<room_availability>().FindAsync(id);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll(bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.room_availabilities);
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<room_availability>().ToListAsync());
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] room_availability item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        await DbContext.Set<room_availability>().AddAsync(item);
        await DbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = item.id }, item);
    }

    [HttpPut("Update")]
    public async Task<IActionResult> Update([FromBody] room_availability item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        DbContext.Set<room_availability>().Update(item);
        await DbContext.SaveChangesAsync();
        return Ok(item);
    }

    [HttpDelete("Delete")]
    public async Task<IActionResult> Delete(uint id, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        var entity = await DbContext.Set<room_availability>().FindAsync(id);
        if (entity is null) return NotFound();

        DbContext.Set<room_availability>().Remove(entity);
        await DbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("GetByRoomId")]
    public async Task<IActionResult> GetByRoomId(uint roomId, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.room_availabilities.Where(a => a.room_id == roomId));
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<room_availability>().Where(a => a.room_id == roomId).ToListAsync());
    }

    [HttpGet("GetByScheduleRange")]
    public async Task<IActionResult> GetByScheduleRange(DateTime startSchedule, DateTime endSchedule, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.room_availabilities.Where(a => a.start_schedule >= startSchedule && a.end_schedule <= endSchedule));
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<room_availability>().Where(a => a.start_schedule >= startSchedule && a.end_schedule <= endSchedule).ToListAsync());
    }

    [HttpGet("GetOverlappingAvailabilities")]
    public async Task<IActionResult> GetOverlappingAvailabilities(DateTime startDate, DateTime endDate, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            return Ok(JsonContext!.room_availabilities.Where(a => a.start_schedule < endDate && a.end_schedule > startDate));
        }

        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<room_availability>().Where(a => a.start_schedule < endDate && a.end_schedule > startDate).ToListAsync());
    }

    [HttpGet("GetRoomByAvailabilityId")]
    public async Task<IActionResult> GetRoomByAvailabilityId(uint availabilityId, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            var availability = JsonContext!.room_availabilities.FirstOrDefault(a => a.id == availabilityId);
            if (availability is null) return NotFound();
            return Ok(JsonContext.rooms.FirstOrDefault(r => r.id == availability.room_id));
        }

        if (DbContext is null) return DbBackendMissing();
        var room = await DbContext.Set<room_availability>().Where(a => a.id == availabilityId)
            .Join(DbContext.Set<room>(), a => a.room_id, r => r.id, (_, r) => r)
            .FirstOrDefaultAsync();
        return room is null ? NotFound() : Ok(room);
    }

    [HttpGet("GetAllAvailabilities")]
    public async Task<IActionResult> GetAllAvailabilities(bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.room_availabilities);
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<room_availability>().ToListAsync());
    }
}
