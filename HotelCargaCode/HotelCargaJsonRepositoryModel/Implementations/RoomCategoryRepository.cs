using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelCargaJsonRepositoryModel.Interfaces;
using Entities = HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCargaJsonRepositoryModel.Implementations;

/// <summary>
/// Implementation of IRoomCategoryRepository
/// </summary>
public class RoomCategoryRepository : IRoomCategoryRepository
{
    private readonly JsonDataContext _context;

    public RoomCategoryRepository(JsonDataContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public Task<Entities.room_category?> GetByIdAsync(byte id)
    {
        return Task.FromResult(_context.room_categories.FirstOrDefault(rc => rc.id == id));
    }

    public Task<IEnumerable<Entities.room_category>> GetByCategoryNamePartialAsync(string categoryName)
    {
        IEnumerable<Entities.room_category> results = _context.room_categories
            .Where(rc => rc.category_name!.Contains(categoryName))
            .ToList();

        return Task.FromResult(results);
    }

    public Task<IEnumerable<Entities.room_category>> GetByDescriptionPartialAsync(string description)
    {
        IEnumerable<Entities.room_category> results = _context.room_categories
            .Where(rc => rc.description!.Contains(description))
            .ToList();

        return Task.FromResult(results);
    }

    public Task<IEnumerable<Entities.room_category>> GetByAmenitiesPartialAsync(string amenities)
    {
        IEnumerable<Entities.room_category> results = _context.room_categories
            .Where(rc => rc.amenities!.Contains(amenities))
            .ToList();

        return Task.FromResult(results);
    }

    public Task<IEnumerable<uint>> GetRoomNumbersByCategoryNameAsync(string categoryName)
    {
        var categoryId = _context.room_categories
            .FirstOrDefault(rc => rc.category_name == categoryName)
            ?.id;

        if (categoryId is null)
        {
            return Task.FromResult(Enumerable.Empty<uint>());
        }

        IEnumerable<uint> results = _context.rooms
            .Where(r => r.category_id == categoryId)
            .Select(r => r.room_number)
            .ToList();

        return Task.FromResult(results);
    }

    public Task<IEnumerable<string>> GetCategoryNamesByRoomNumberAsync(uint roomNumber)
    {
        var room = _context.rooms
            .FirstOrDefault(r => r.room_number == roomNumber);

        if (room is null)
        {
            return Task.FromResult(Enumerable.Empty<string>());
        }

        var category = _context.room_categories.FirstOrDefault(rc => rc.id == room.category_id);
        return Task.FromResult(category is null ? Enumerable.Empty<string>() : new[] { category.category_name });
    }

    public Task<IEnumerable<Entities.room_category>> GetAllCategoriesAsync()
    {
        IEnumerable<Entities.room_category> results = _context.room_categories.ToList();
        return Task.FromResult(results);
    }
}



