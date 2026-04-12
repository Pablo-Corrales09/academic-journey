using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelCarga.RepositoryModel.Interfaces;
using Microsoft.EntityFrameworkCore;
using Entities = HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCarga.RepositoryModel.Implementations;

/// <summary>
/// Implementation of IRoomCategoryRepository
/// </summary>
public class RoomCategoryRepository : IRoomCategoryRepository
{
    private readonly Entities.HotelCargaContext _context;

    public RoomCategoryRepository(Entities.HotelCargaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Entities.room_category?> GetByIdAsync(byte id)
    {
        return await _context.room_categories.FirstOrDefaultAsync(rc => rc.id == id);
    }

    public async Task<IEnumerable<Entities.room_category>> GetByCategoryNamePartialAsync(string categoryName)
    {
        return await _context.room_categories
            .Where(rc => rc.category_name!.Contains(categoryName))
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.room_category>> GetByDescriptionPartialAsync(string description)
    {
        return await _context.room_categories
            .Where(rc => rc.description!.Contains(description))
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.room_category>> GetByAmenitiesPartialAsync(string amenities)
    {
        return await _context.room_categories
            .Where(rc => rc.amenities!.Contains(amenities))
            .ToListAsync();
    }

    public async Task<IEnumerable<uint>> GetRoomNumbersByCategoryNameAsync(string categoryName)
    {
        return await _context.rooms
            .Where(r => r.category!.category_name == categoryName)
            .Select(r => r.room_number)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetCategoryNamesByRoomNumberAsync(uint roomNumber)
    {
        var room = await _context.rooms
            .Include(r => r.category)
            .FirstOrDefaultAsync(r => r.room_number == roomNumber);
        
        return room?.category != null ? new[] { room.category.category_name ?? "" } : Enumerable.Empty<string>();
    }

    public async Task<IEnumerable<Entities.room_category>> GetAllCategoriesAsync()
    {
        return await _context.room_categories.ToListAsync();
    }
}
