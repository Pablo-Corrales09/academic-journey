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
public class BookingHistoryController : BaseApiController
{
    public BookingHistoryController(HotelCargaContext? dbContext = null, JsonDataContext? jsonContext = null)
        : base(dbContext, jsonContext)
    {
    }

    [HttpGet("GetById")]
    public async Task<IActionResult> GetById(uint id, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            return Ok(JsonContext!.booking_histories.FirstOrDefault(h => h.id == id));
        }

        if (DbContext is null) return DbBackendMissing();
        var entity = await DbContext.Set<booking_history>().FindAsync(id);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll(bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.booking_histories);
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<booking_history>().ToListAsync());
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] booking_history item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        await DbContext.Set<booking_history>().AddAsync(item);
        await DbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = item.id }, item);
    }

    [HttpPut("Update")]
    public async Task<IActionResult> Update([FromBody] booking_history item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        DbContext.Set<booking_history>().Update(item);
        await DbContext.SaveChangesAsync();
        return Ok(item);
    }

    [HttpDelete("Delete")]
    public async Task<IActionResult> Delete(uint id, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        var entity = await DbContext.Set<booking_history>().FindAsync(id);
        if (entity is null) return NotFound();

        DbContext.Set<booking_history>().Remove(entity);
        await DbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("GetByBookingId")]
    public async Task<IActionResult> GetByBookingId(uint bookingId, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            return Ok(JsonContext!.booking_histories.Where(h => h.booking_id == bookingId));
        }

        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<booking_history>().Where(h => h.booking_id == bookingId).ToListAsync());
    }

    [HttpGet("GetByCustomerId")]
    public async Task<IActionResult> GetByCustomerId(uint customerId, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            return Ok(JsonContext!.booking_histories.Where(h => h.customer_id == customerId));
        }

        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<booking_history>().Where(h => h.customer_id == customerId).ToListAsync());
    }

    [HttpGet("GetByRoomId")]
    public async Task<IActionResult> GetByRoomId(uint roomId, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            return Ok(JsonContext!.booking_histories.Where(h => h.room_id == roomId));
        }

        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<booking_history>().Where(h => h.room_id == roomId).ToListAsync());
    }

    [HttpGet("GetByActionType")]
    public async Task<IActionResult> GetByActionType(string actionType, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            return Ok(JsonContext!.booking_histories.Where(h => h.action_type == actionType));
        }

        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<booking_history>().Where(h => h.action_type == actionType).ToListAsync());
    }

    [HttpGet("GetByDateRange")]
    public async Task<IActionResult> GetByDateRange(DateTime startDate, DateTime endDate, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            return Ok(JsonContext!.booking_histories.Where(h => h.created_at >= startDate && h.created_at <= endDate));
        }

        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<booking_history>().Where(h => h.created_at >= startDate && h.created_at <= endDate).ToListAsync());
    }
}
