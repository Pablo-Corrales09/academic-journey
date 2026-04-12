using System.Linq;
using System.Threading.Tasks;
using HotelCargaContext = HotelCarga.DbModel.HotelCargaContext;
using HotelCarga.HotelCarga.DbModel.Entities;
using HotelCarga.DbModel;
using HotelCargaJsonRepositoryModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

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
        customer? customerItem = null;
        
        if (UseJsonBackend(useJson))
        {
            customerItem = JsonContext?.customers.FirstOrDefault(c => c.id == id);           

            if (customerItem != null)
            {
                var user = JsonContext?.users.FirstOrDefault(u => u.id == customerItem.user_id);                
                if (user is not null)
                {
                    var status = JsonContext?.user_statuses.FirstOrDefault(s => s.id == user.status_id);
                    if (status is not null) user.status = status;

                    var role = JsonContext?.roles.FirstOrDefault(r => r.id == user.role_id);
                    if (role is not null) user.role = role;
                    customerItem.user = user;
                }
            }
        }
        else
        {
            if (DbContext is null) return DbBackendMissing();

            customerItem = await DbContext.Set<customer>()
                .Include(c => c.user)
                    .ThenInclude(u => u!.status)
                .Include(c => c.user)
                    .ThenInclude(u => u!.role)
                .FirstOrDefaultAsync(c => c.id == id);
        }
        if (customerItem is null) return NotFound();
        return Ok(BuildUserResponse(customerItem));       

    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll(bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            var customers = JsonContext!.customers;
            foreach (var customerItem in customers)
            {
                var user = JsonContext.users.FirstOrDefault(u => u.id == customerItem.user_id);
                var status = JsonContext.user_statuses.FirstOrDefault(s => s.id == user?.status_id);
            }
            return Ok(customers.Select(BuildUserResponse).ToList());        
        }
        if (DbContext is null) return DbBackendMissing();
        var entities = await DbContext.Set<customer>()
        .Include(c => c.user)
        .ThenInclude(u => u!.status)
        .Include(c => c.user)
        .ThenInclude(u => u!.role)
        .ToListAsync();
        return Ok(entities.Select(BuildUserResponse).ToList());
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] customer item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        var userValidation = await ValidateNewCustomerAsync(item);

        if (!userValidation.IsValid)
        {
        return Conflict(new { error = true, message = userValidation.ErrorMessage });  
        }

        await DbContext.Set<customer>().AddAsync(item);
        await DbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = item.id }, item);
    }

    //Valida si el número de documento o correo ya está ocupado por un usuario.
    public async Task<(bool IsValid, string ErrorMessage)> ValidateNewCustomerAsync(customer item)
    {
        bool userAlreadyTaken = await DbContext!.Set<customer>()
            .AnyAsync(c => c.user_id == item.user_id);
        
        if (userAlreadyTaken) 
            {
                return (false, $"El usuario {item.user_id} ya tiene un perfil de cliente asignado.");
            }
        
        bool documentExists = await DbContext.Set<customer>()
        .AnyAsync(c => c.document_number == item.document_number);
        
        if (documentExists) 
        {
            return (false, $"El número de documento '{item.document_number}' ya está registrado.");
        }

        return (true, string.Empty);
    }


    [HttpPut("Update")]
    public async Task<IActionResult> Update([FromBody] customer item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        // 1. Validar que el cliente enviado en el JSON tenga un ID mayor a 0
        if (item.id == 0)
        {
            return BadRequest(new { error = true, message = "El ID del cliente es necesario para actualizar." });
        }

        var existingCustomer = await DbContext.Set<customer>().FindAsync(item.id);
        if (existingCustomer is null)
        {
            return NotFound(new { error = true, message = $"No se encontró ningún cliente con el ID {item.id}." });
        }

        existingCustomer.document_number = item.document_number;
        existingCustomer.first_name = item.first_name;
        existingCustomer.last_name = item.last_name;
        existingCustomer.phone = item.phone;
        existingCustomer.address = item.address;
        existingCustomer.city = item.city;
        existingCustomer.country = item.country;

        DbContext.Set<customer>().Update(existingCustomer);
        await DbContext.SaveChangesAsync();

        return Ok(new
        {
            error = false,
            message = "Cliente actualizado exitosamente",
            data = BuildUserResponse(existingCustomer)  
        });
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

    public static object BuildUserResponse(customer customerItem)
    {
        return new
        {
            customerItem.id,
            customerItem.first_name,
            customerItem.last_name, 
            customerItem.document_number,
            customerItem.phone,
            customerItem.country,
            customerItem.city,
            customerItem.address,
            username = customerItem.user?.username,
            email = customerItem.user?.email,
            role = customerItem.user?.role?.role_name,
            status = customerItem.user?.status?.status_name,
            customerItem.bookings,
            customerItem.waiting_queues           
        };
    }

}
