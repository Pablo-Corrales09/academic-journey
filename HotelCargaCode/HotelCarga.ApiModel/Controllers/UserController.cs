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
public class UserController : BaseApiController
{
    public UserController(HotelCargaContext? dbContext = null, JsonDataContext? jsonContext = null)
        : base(dbContext, jsonContext)
    {
    }

    [HttpGet("GetById")]
    public async Task<IActionResult> GetById(uint id, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            var userItem = JsonContext!.users.FirstOrDefault(u => u.id == id);
            if (userItem is null) return NotFound();
            userItem.role = JsonContext.roles.FirstOrDefault(r => r.id == userItem.role_id)!;
            userItem.status = JsonContext.user_statuses.FirstOrDefault(s => s.id == userItem.status_id)!;
            userItem.customer = JsonContext.customers.FirstOrDefault(c => c.user_id == userItem.id)!;
            return Ok(BuildUserResponse(userItem));
        }
        if (DbContext is null) return DbBackendMissing();
        var entity = await DbContext.Set<user>()
            .Include(u => u.customer)
            .Include(u => u.role)
            .Include(u => u.status)
            .FirstOrDefaultAsync(u => u.id == id);
        return entity is null ? NotFound() : Ok(BuildUserResponse(entity));
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll(bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            var users = JsonContext!.users;
            foreach (var userItem in users)
            {
                userItem.role = JsonContext.roles.FirstOrDefault(r => r.id == userItem.role_id)!;
                userItem.status = JsonContext.user_statuses.FirstOrDefault(s => s.id == userItem.status_id)!;
                userItem.customer = JsonContext.customers.FirstOrDefault(c => c.user_id == userItem.id)!;
            }

            return Ok(users.Select(BuildUserResponse).ToList());
        }
        if (DbContext is null) return DbBackendMissing();
        var entities = await DbContext.Set<user>()
            .Include(u => u.customer)
            .Include(u => u.role)
            .Include(u => u.status)
            .ToListAsync();
        return Ok(entities.Select(BuildUserResponse).ToList());
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] user item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        await DbContext.Set<user>().AddAsync(item);
        await DbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = item.id }, BuilderResponseCreate(item));
    }

    [HttpPut("Update")]
    public async Task<IActionResult> Update([FromBody] user item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        DbContext.Set<user>().Update(item);
        await DbContext.SaveChangesAsync();
        return Ok(BuilderResponseUpdate(item));
    }

    [HttpDelete("Delete")]
    public async Task<IActionResult> Delete(uint id, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        var entity = await DbContext.Set<user>().FindAsync(id);
        if (entity is null) return NotFound();

        DbContext.Set<user>().Remove(entity);
        await DbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("GetUsernameById")]
    public async Task<IActionResult> GetUsernameById(uint id, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.users.FirstOrDefault(u => u.id == id)?.username);
        if (DbContext is null) return DbBackendMissing();
        var username = await DbContext.Set<user>().Where(u => u.id == id).Select(u => u.username).FirstOrDefaultAsync();
        return Ok(username);
    }

    [HttpGet("GetUserWithRoleByUsername")]
    public async Task<IActionResult> GetUserWithRoleByUsername(string username, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            var userItem = JsonContext!.users.FirstOrDefault(u => u.username == username);
            if (userItem is null) return NotFound();
            userItem.role = JsonContext.roles.FirstOrDefault(r => r.id == userItem.role_id)!;
            userItem.status = JsonContext.user_statuses.FirstOrDefault(s => s.id == userItem.status_id)!;
            userItem.customer = JsonContext.customers.FirstOrDefault(c => c.user_id == userItem.id)!;
            return Ok(BuildUserAuthResponse(userItem));
        }

        if (DbContext is null) return DbBackendMissing();
        var userWithRole = await DbContext.Set<user>()
            .Include(u => u.role)
            .Include(u => u.status)
            .Include(u => u.customer)
            .FirstOrDefaultAsync(u => u.username == username);
        return userWithRole is null ? NotFound() : Ok(BuildUserAuthResponse(userWithRole));
    }

    [HttpGet("GetUserWithRoleByEmail")]
    public async Task<IActionResult> GetUserWithRoleByEmail(string email, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            var userItem = JsonContext!.users.FirstOrDefault(u => u.email == email);
            if (userItem is null) return NotFound();
            userItem.role = JsonContext.roles.FirstOrDefault(r => r.id == userItem.role_id)!;
            userItem.status = JsonContext.user_statuses.FirstOrDefault(s => s.id == userItem.status_id)!;
            userItem.customer = JsonContext.customers.FirstOrDefault(c => c.user_id == userItem.id)!;
            return Ok(BuildUserAuthResponse(userItem));
        }

        if (DbContext is null) return DbBackendMissing();
        var userWithRole = await DbContext.Set<user>()
            .Include(u => u.role)
            .Include(u => u.status)
            .Include(u => u.customer)
            .FirstOrDefaultAsync(u => u.email == email);
        return userWithRole is null ? NotFound() : Ok(BuildUserAuthResponse(userWithRole));
    }

    [HttpGet("UsernameExists")]
    public async Task<IActionResult> UsernameExists(string username, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.users.Any(u => u.username == username));
        if (DbContext is null) return DbBackendMissing();
        var exists = await DbContext.Set<user>().AnyAsync(u => u.username == username);
        return Ok(exists);
    }

    [HttpGet("EmailExists")]
    public async Task<IActionResult> EmailExists(string email, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.users.Any(u => u.email == email));
        if (DbContext is null) return DbBackendMissing();
        var exists = await DbContext.Set<user>().AnyAsync(u => u.email == email);
        return Ok(exists);
    }

    private static object BuildUserResponse(user userItem)
    {
        return new
        {
            userItem.id,
            userItem.username,
            userItem.email,
            role = userItem.role?.role_name ,
            status = userItem.status?.status_name,
            customer = userItem.customer?.id
        };
    }

    private static object BuildUserAuthResponse(user userItem)
    {
        return new
        {
            userItem.id,
            userItem.username,
            userItem.email,
            userItem.password_hash,
            userItem.role_id,
            userItem.status_id,
            userItem.created_at,
            userItem.updated_at,
            role = userItem.role is null
                ? null
                : new
                {
                    userItem.role.id,
                    userItem.role.role_name,
                    userItem.role.description,
                    userItem.role.created_at
                },
            status = userItem.status is null
                ? null
                : new
                {
                    userItem.status.id,
                    userItem.status.status_name,
                    userItem.status.description,
                    userItem.status.created_at
                },
            customer = userItem.customer is null
                ? null
                : new
                {
                    userItem.customer.id,
                    user_id = userItem.customer.user_id,
                    document_number = userItem.customer.document_number,
                    first_name = userItem.customer.first_name,
                    last_name = userItem.customer.last_name,
                    phone = userItem.customer.phone,
                    address = userItem.customer.address,
                    city = userItem.customer.city,
                    country = userItem.customer.country,
                    created_at = userItem.customer.created_at,
                    updated_at = userItem.customer.updated_at
                }
        };
    }

  private static object BuilderResponseCreate(user userItem)
{
    if (userItem is null) 
    {
        return new 
        { 
            error = true,
            message = "No se pudo procesar la respuesta porque el usuario está vacío." 
        };
    }
    return new
    {
        userItem.id,
        message = "Usuario " + userItem.username + " creado exitosamente",       
    };
}

  private static object BuilderResponseUpdate(user userItem)
{
    if (userItem is null) 
    {
        return new 
        { 
            error = true,
            message = "No se pudo procesar la respuesta porque el usuario está vacío." 
        };
    }
    return new
    {       
        message = "Usuario actualizado exitosamente",
        userItem.id,
        userItem.username,
        userItem.email,
        userItem.role_id,
        userItem.status_id    
    };
}

}
