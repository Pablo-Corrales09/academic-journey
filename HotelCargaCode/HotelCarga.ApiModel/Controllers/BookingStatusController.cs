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
public class BookingStatusController : BaseApiController
{
    public BookingStatusController(HotelCargaContext? dbContext = null, JsonDataContext? jsonContext = null)
        : base(dbContext, jsonContext)
    {
    }

    [HttpGet("GetById")]
    public async Task<IActionResult> GetById(byte id, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.booking_statuses.FirstOrDefault(s => s.id == id));
        if (DbContext is null) return DbBackendMissing();
        var status = await DbContext.Set<booking_status>().FindAsync(id);
        return status is null ? NotFound() : Ok(status);
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll(bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.booking_statuses);
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<booking_status>().ToListAsync());
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] booking_status item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        await DbContext.Set<booking_status>().AddAsync(item);
        await DbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = item.id }, item);
    }

    [HttpPut("Update")]
    public async Task<IActionResult> Update([FromBody] booking_status item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        DbContext.Set<booking_status>().Update(item);
        await DbContext.SaveChangesAsync();
        return Ok(item);
    }

    [HttpDelete("Delete")]
    public async Task<IActionResult> Delete(byte id, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        var status = await DbContext.Set<booking_status>().FindAsync(id);
        if (status is null) return NotFound();

        DbContext.Set<booking_status>().Remove(status);
        await DbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("GetByStatusName")]
    public async Task<IActionResult> GetByStatusName(string statusName, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.booking_statuses.FirstOrDefault(s => s.status_name == statusName));
        if (DbContext is null) return DbBackendMissing();
        var status = await DbContext.Set<booking_status>().FirstOrDefaultAsync(s => s.status_name == statusName);
        return status is null ? NotFound() : Ok(status);
    }

    [HttpGet("GetBookingHistoryIdsByStatusName")]
    public async Task<IActionResult> GetBookingHistoryIdsByStatusName(string statusName, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            var status = JsonContext!.booking_statuses.FirstOrDefault(s => s.status_name == statusName);
            if (status is null) return Ok(Enumerable.Empty<uint>());
            return Ok(JsonContext.booking_histories.Where(h => h.status_id == status.id).Select(h => h.id));
        }

        if (DbContext is null) return DbBackendMissing();
        var statusId = await DbContext.Set<booking_status>().Where(s => s.status_name == statusName).Select(s => (byte?)s.id).FirstOrDefaultAsync();
        if (statusId is null) return Ok(Enumerable.Empty<uint>());
        var ids = await DbContext.Set<booking_history>().Where(h => h.status_id == statusId.Value).Select(h => h.id).ToListAsync();
        return Ok(ids);
    }

    [HttpGet("GetStatusNameByBookingId")]
    public async Task<IActionResult> GetStatusNameByBookingId(uint bookingId, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            var booking = JsonContext!.bookings.FirstOrDefault(b => b.id == bookingId);
            return booking is null ? NotFound() : Ok(JsonContext.booking_statuses.FirstOrDefault(s => s.id == booking.status_id)?.status_name);
        }

        if (DbContext is null) return DbBackendMissing();
        var result = await DbContext.Set<booking>().Where(b => b.id == bookingId)
            .Join(DbContext.Set<booking_status>(), b => b.status_id, s => s.id, (b, s) => s.status_name)
            .FirstOrDefaultAsync();
        return Ok(result);
    }

    [HttpGet("GetAllStatuses")]
    public async Task<IActionResult> GetAllStatuses(bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.booking_statuses);
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<booking_status>().ToListAsync());
    }
}
