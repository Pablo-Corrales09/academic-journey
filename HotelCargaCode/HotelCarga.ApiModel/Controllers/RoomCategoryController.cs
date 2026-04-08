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
public class RoomCategoryController : BaseApiController
{
    public RoomCategoryController(HotelCargaContext? dbContext = null, JsonDataContext? jsonContext = null)
        : base(dbContext, jsonContext)
    {
    }

    [HttpGet("GetById")]
    public async Task<IActionResult> GetById(byte id, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.room_categories.FirstOrDefault(c => c.id == id));
        if (DbContext is null) return DbBackendMissing();
        var entity = await DbContext.Set<room_category>().FindAsync(id);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll(bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.room_categories);
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<room_category>().ToListAsync());
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] room_category item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        await DbContext.Set<room_category>().AddAsync(item);
        await DbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = item.id }, item);
    }

    [HttpPut("Update")]
    public async Task<IActionResult> Update([FromBody] room_category item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        DbContext.Set<room_category>().Update(item);
        await DbContext.SaveChangesAsync();
        return Ok(item);
    }

    [HttpDelete("Delete")]
    public async Task<IActionResult> Delete(byte id, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        var entity = await DbContext.Set<room_category>().FindAsync(id);
        if (entity is null) return NotFound();

        DbContext.Set<room_category>().Remove(entity);
        await DbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("GetByCategoryNamePartial")]
    public async Task<IActionResult> GetByCategoryNamePartial(string categoryName, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.room_categories.Where(c => c.category_name.Contains(categoryName)));
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<room_category>().Where(c => c.category_name.Contains(categoryName)).ToListAsync());
    }

    [HttpGet("GetByDescriptionPartial")]
    public async Task<IActionResult> GetByDescriptionPartial(string description, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.room_categories.Where(c => c.description.Contains(description)));
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<room_category>().Where(c => c.description.Contains(description)).ToListAsync());
    }

    [HttpGet("GetByAmenitiesPartial")]
    public async Task<IActionResult> GetByAmenitiesPartial(string amenities, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.room_categories.Where(c => c.amenities.Contains(amenities)));
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<room_category>().Where(c => c.amenities.Contains(amenities)).ToListAsync());
    }

    [HttpGet("GetRoomNumbersByCategoryName")]
    public async Task<IActionResult> GetRoomNumbersByCategoryName(string categoryName, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            var category = JsonContext!.room_categories.FirstOrDefault(c => c.category_name == categoryName);
            if (category is null) return Ok(Enumerable.Empty<uint>());
            return Ok(JsonContext.rooms.Where(r => r.category_id == category.id).Select(r => r.room_number));
        }

        if (DbContext is null) return DbBackendMissing();
        var categoryId = await DbContext.Set<room_category>().Where(c => c.category_name == categoryName).Select(c => (byte?)c.id).FirstOrDefaultAsync();
        if (categoryId is null) return Ok(Enumerable.Empty<uint>());
        return Ok(await DbContext.Set<room>().Where(r => r.category_id == categoryId.Value).Select(r => r.room_number).ToListAsync());
    }

    [HttpGet("GetCategoryNamesByRoomNumber")]
    public async Task<IActionResult> GetCategoryNamesByRoomNumber(uint roomNumber, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            var room = JsonContext!.rooms.FirstOrDefault(r => r.room_number == roomNumber);
            if (room is null) return Ok(Enumerable.Empty<string>());
            return Ok(JsonContext.room_categories.Where(c => c.id == room.category_id).Select(c => c.category_name));
        }

        if (DbContext is null) return DbBackendMissing();
        var roomEntity = await DbContext.Set<room>().FirstOrDefaultAsync(r => r.room_number == roomNumber);
        if (roomEntity is null) return Ok(Enumerable.Empty<string>());
        return Ok(await DbContext.Set<room_category>().Where(c => c.id == roomEntity.category_id).Select(c => c.category_name).ToListAsync());
    }

    [HttpGet("GetAllCategories")]
    public async Task<IActionResult> GetAllCategories(bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(JsonContext!.room_categories);
        if (DbContext is null) return DbBackendMissing();
        return Ok(await DbContext.Set<room_category>().ToListAsync());
    }
}
