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
public class RoleController : BaseApiController
{
    public RoleController(HotelCargaContext? dbContext = null, JsonDataContext? jsonContext = null)
        : base(dbContext, jsonContext)
    {
    }

    [HttpGet("GetById")]
    public async Task<IActionResult> GetById(byte id, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.roles.FirstOrDefault(r => r.id == id));
        if (DbContext is null) return DbBackendMissing();
        var entity = await DbContext.Set<role>().FindAsync(id);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll(bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.roles);
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<role>().ToListAsync());
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] role item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        await DbContext.Set<role>().AddAsync(item);
        await DbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = item.id }, item);
    }

    [HttpPut("Update")]
    public async Task<IActionResult> Update([FromBody] role item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        DbContext.Set<role>().Update(item);
        await DbContext.SaveChangesAsync();
        return Ok(item);
    }

    [HttpDelete("Delete")]
    public async Task<IActionResult> Delete(byte id, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        var entity = await DbContext.Set<role>().FindAsync(id);
        if (entity is null) return NotFound();

        DbContext.Set<role>().Remove(entity);
        await DbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("GetRoleNameById")]
    public async Task<IActionResult> GetRoleNameById(byte id, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.roles.FirstOrDefault(r => r.id == id)?.role_name);
        if (DbContext is null) return DbBackendMissing();
        var result = await DbContext.Set<role>().Where(r => r.id == id).Select(r => r.role_name).FirstOrDefaultAsync();
        return Ok(result);
    }

    [HttpGet("GetDescriptionByRoleName")]
    public async Task<IActionResult> GetDescriptionByRoleName(string roleName, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.roles.FirstOrDefault(r => r.role_name == roleName)?.description);
        if (DbContext is null) return DbBackendMissing();
        var result = await DbContext.Set<role>().Where(r => r.role_name == roleName).Select(r => r.description).FirstOrDefaultAsync();
        return Ok(result);
    }

    [HttpGet("GetUserIdsByRoleName")]
    public async Task<IActionResult> GetUserIdsByRoleName(string roleName, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            var role = JsonContext!.roles.FirstOrDefault(r => r.role_name == roleName);
            if (role is null) return Ok(Enumerable.Empty<uint>());
            return Ok(JsonContext.users.Where(u => u.role_id == role.id).Select(u => u.id));
        }

        if (DbContext is null) return DbBackendMissing();
        var roleId = await DbContext.Set<role>().Where(r => r.role_name == roleName).Select(r => (byte?)r.id).FirstOrDefaultAsync();
        if (roleId is null) return Ok(Enumerable.Empty<uint>());
        return Ok(await DbContext.Set<user>().Where(u => u.role_id == roleId.Value).Select(u => u.id).ToListAsync());
    }

    [HttpGet("GetAllRoles")]
    public async Task<IActionResult> GetAllRoles(bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.roles);
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<role>().ToListAsync());
    }
}
