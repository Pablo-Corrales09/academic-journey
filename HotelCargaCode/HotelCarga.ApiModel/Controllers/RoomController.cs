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
public class RoomController : BaseApiController
{
    public RoomController(HotelCargaContext? dbContext = null, JsonDataContext? jsonContext = null)
        : base(dbContext, jsonContext)
    {
    }

    [HttpGet("GetById")]
    public async Task<IActionResult> GetById(uint id, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.rooms.FirstOrDefault(r => r.id == id));
        if (DbContext is null) return DbBackendMissing();
        var entity = await DbContext.Set<room>().FindAsync(id);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll(bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.rooms);
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<room>().ToListAsync());
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] room item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        await DbContext.Set<room>().AddAsync(item);
        await DbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = item.id }, item);
    }

    [HttpPut("Update")]
    public async Task<IActionResult> Update([FromBody] room item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        DbContext.Set<room>().Update(item);
        await DbContext.SaveChangesAsync();
        return Ok(item);
    }

    [HttpDelete("Delete")]
    public async Task<IActionResult> Delete(uint id, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        var entity = await DbContext.Set<room>().FindAsync(id);
        if (entity is null) return NotFound();

        DbContext.Set<room>().Remove(entity);
        await DbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("GetByRoomNumber")]
    public async Task<IActionResult> GetByRoomNumber(uint roomNumber, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.rooms.FirstOrDefault(r => r.room_number == roomNumber));
        if (DbContext is null) return DbBackendMissing();
        var room = await DbContext.Set<room>().FirstOrDefaultAsync(r => r.room_number == roomNumber);
        return room is null ? NotFound() : Ok(room);
    }

    [HttpGet("GetByNightlyRateRange")]
    public async Task<IActionResult> GetByNightlyRateRange(decimal minRate, decimal maxRate, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.rooms.Where(r => r.nightly_rate >= minRate && r.nightly_rate <= maxRate));
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<room>().Where(r => r.nightly_rate >= minRate && r.nightly_rate <= maxRate).ToListAsync());
    }

    [HttpGet("GetByStatusId")]
    public async Task<IActionResult> GetByStatusId(byte statusId, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.rooms.Where(r => r.status_id == statusId));
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<room>().Where(r => r.status_id == statusId).ToListAsync());
    }

    [HttpGet("GetByCategoryId")]
    public async Task<IActionResult> GetByCategoryId(byte categoryId, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.rooms.Where(r => r.category_id == categoryId));
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<room>().Where(r => r.category_id == categoryId).ToListAsync());
    }

    [HttpGet("GetByFloorNumber")]
    public async Task<IActionResult> GetByFloorNumber(byte floorNumber, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.rooms.Where(r => r.floor_number == floorNumber));
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<room>().Where(r => r.floor_number == floorNumber).ToListAsync());
    }

    [HttpGet("GetBookingsByRoomId")]
    public async Task<IActionResult> GetBookingsByRoomId(uint roomId, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.bookings.Where(b => b.room_id == roomId));
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<booking>().Where(b => b.room_id == roomId).ToListAsync());
    }

    [HttpGet("GetBookingHistoriesByRoomId")]
    public async Task<IActionResult> GetBookingHistoriesByRoomId(uint roomId, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.booking_histories.Where(h => h.room_id == roomId));
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<booking_history>().Where(h => h.room_id == roomId).ToListAsync());
    }

    [HttpGet("GetAvailabilitiesByRoomId")]
    public async Task<IActionResult> GetAvailabilitiesByRoomId(uint roomId, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.room_availabilities.Where(a => a.room_id == roomId));
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<room_availability>().Where(a => a.room_id == roomId).ToListAsync());
    }
}
