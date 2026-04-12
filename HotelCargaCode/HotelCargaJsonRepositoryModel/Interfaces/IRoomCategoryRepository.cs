using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCargaJsonRepositoryModel.Interfaces;

/// <summary>
/// Repository interface for room_category entity
/// </summary>
public interface IRoomCategoryRepository
{
    /// <summary>
    /// Query by id (PK)
    /// </summary>
    Task<room_category?> GetByIdAsync(byte id);

    /// <summary>
    /// Query by partial category_name (string)
    /// </summary>
    Task<IEnumerable<room_category>> GetByCategoryNamePartialAsync(string categoryName);

    /// <summary>
    /// Query by partial description (string)
    /// </summary>
    Task<IEnumerable<room_category>> GetByDescriptionPartialAsync(string description);

    /// <summary>
    /// Query by partial amenities
    /// </summary>
    Task<IEnumerable<room_category>> GetByAmenitiesPartialAsync(string amenities);

    /// <summary>
    /// Given a category_name (string) query all the room_numbers (uint) related to the category_name
    /// </summary>
    Task<IEnumerable<uint>> GetRoomNumbersByCategoryNameAsync(string categoryName);

    /// <summary>
    /// Given a room_number (uint) query all the category_name (string) related to the room_number
    /// </summary>
    Task<IEnumerable<string>> GetCategoryNamesByRoomNumberAsync(uint roomNumber);

    /// <summary>
    /// Get a list of all room_category entities (useful for dropdown menus)
    /// </summary>
    Task<IEnumerable<room_category>> GetAllCategoriesAsync();
}

