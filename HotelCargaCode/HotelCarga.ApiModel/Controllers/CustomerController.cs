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
public class CustomerController : BaseApiController
{
    public CustomerController(HotelCargaContext? dbContext = null, JsonDataContext? jsonContext = null)
        : base(dbContext, jsonContext)
    {
    }

    [HttpGet("GetById")]
    public async Task<IActionResult> GetById(uint id, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.customers.FirstOrDefault(c => c.id == id));
        if (DbContext is null) return DbBackendMissing();
        var customer = await DbContext.Set<customer>().FindAsync(id);
        return customer is null ? NotFound() : Ok(customer);
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll(bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.customers);
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<customer>().ToListAsync());
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] customer item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        await DbContext.Set<customer>().AddAsync(item);
        await DbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = item.id }, item);
    }

    [HttpPut("Update")]
    public async Task<IActionResult> Update([FromBody] customer item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        DbContext.Set<customer>().Update(item);
        await DbContext.SaveChangesAsync();
        return Ok(item);
    }

    [HttpDelete("Delete")]
    public async Task<IActionResult> Delete(uint id, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        var entity = await DbContext.Set<customer>().FindAsync(id);
        if (entity is null) return NotFound();

        DbContext.Set<customer>().Remove(entity);
        await DbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("SearchByName")]
    public async Task<IActionResult> SearchByName(string searchString, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            return Ok(JsonContext!.customers.Where(c => c.first_name.Contains(searchString) || c.last_name.Contains(searchString)));
        }

        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<customer>()
            .Where(c => c.first_name.Contains(searchString) || c.last_name.Contains(searchString))
            .ToListAsync());
    }

    [HttpGet("GetByDocumentNumber")]
    public async Task<IActionResult> GetByDocumentNumber(string documentNumber, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.customers.FirstOrDefault(c => c.document_number == documentNumber));
        if (DbContext is null) return DbBackendMissing();
        var customer = await DbContext.Set<customer>().FirstOrDefaultAsync(c => c.document_number == documentNumber);
        return customer is null ? NotFound() : Ok(customer);
    }

    [HttpGet("GetByPhone")]
    public async Task<IActionResult> GetByPhone(string phone, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.customers.FirstOrDefault(c => c.phone == phone));
        if (DbContext is null) return DbBackendMissing();
        var customer = await DbContext.Set<customer>().FirstOrDefaultAsync(c => c.phone == phone);
        return customer is null ? NotFound() : Ok(customer);
    }

    [HttpGet("GetByDocumentNumberOrPhone")]
    public async Task<IActionResult> GetByDocumentNumberOrPhone(string documentNumber, string phone, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            return Ok(JsonContext!.customers.FirstOrDefault(c => c.document_number == documentNumber || c.phone == phone));
        }

        if (DbContext is null) return DbBackendMissing();
        var customer = await DbContext.Set<customer>().FirstOrDefaultAsync(c => c.document_number == documentNumber || c.phone == phone);
        return customer is null ? NotFound() : Ok(customer);
    }

    [HttpGet("GetByUserId")]
    public async Task<IActionResult> GetByUserId(uint userId, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.customers.FirstOrDefault(c => c.user_id == userId));
        if (DbContext is null) return DbBackendMissing();
        var customer = await DbContext.Set<customer>().FirstOrDefaultAsync(c => c.user_id == userId);
        return customer is null ? NotFound() : Ok(customer);
    }

    [HttpGet("GetBookingIdsByCustomerId")]
    public async Task<IActionResult> GetBookingIdsByCustomerId(uint customerId, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            return Ok(JsonContext!.bookings.Where(b => b.customer_id == customerId).Select(b => b.id));
        }

        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<booking>().Where(b => b.customer_id == customerId).Select(b => b.id).ToListAsync());
    }

    [HttpGet("GetBookingHistoryIdsByCustomerId")]
    public async Task<IActionResult> GetBookingHistoryIdsByCustomerId(uint customerId, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            return Ok(JsonContext!.booking_histories.Where(h => h.customer_id == customerId).Select(h => h.id));
        }

        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<booking_history>().Where(h => h.customer_id == customerId).Select(h => h.id).ToListAsync());
    }

    [HttpGet("GetWaitingQueueIdsByCustomerId")]
    public async Task<IActionResult> GetWaitingQueueIdsByCustomerId(uint customerId, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            return Ok(JsonContext!.waiting_queues.Where(w => w.customer_id == customerId).Select(w => w.id));
        }

        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<waiting_queue>().Where(w => w.customer_id == customerId).Select(w => w.id).ToListAsync());
    }
}
