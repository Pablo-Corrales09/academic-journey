using HotelCarga.HotelCarga.DbModel.Entities;
using HotelCarga.Models.Rooms;
using HotelCarga.RepositoryModel.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HotelCarga.Controllers;

public class RoomController : Controller
{
    private readonly HotelCargaContext _context;
    private readonly IRoomRepository _roomRepository;
    private readonly IRoomStatusRepository _roomStatusRepository;
    private readonly IRoomCategoryRepository _roomCategoryRepository;

    public RoomController(
        HotelCargaContext context,
        IRoomRepository roomRepository,
        IRoomStatusRepository roomStatusRepository,
        IRoomCategoryRepository roomCategoryRepository)
    {
        _context = context;
        _roomRepository = roomRepository;
        _roomStatusRepository = roomStatusRepository;
        _roomCategoryRepository = roomCategoryRepository;
    }

    public async Task<IActionResult> Index(RoomFiltersViewModel filters)
    {
        var query = _context.rooms
            .AsNoTracking()
            .Include(room => room.status)
            .Include(room => room.category)
            .AsQueryable();

        if (filters.RoomNumber.HasValue)
        {
            query = query.Where(room => room.room_number == filters.RoomNumber.Value);
        }

        if (filters.MinRate.HasValue)
        {
            query = query.Where(room => room.nightly_rate >= filters.MinRate.Value);
        }

        if (filters.MaxRate.HasValue)
        {
            query = query.Where(room => room.nightly_rate <= filters.MaxRate.Value);
        }

        if (filters.StatusId.HasValue)
        {
            query = query.Where(room => room.status_id == filters.StatusId.Value);
        }

        if (filters.CategoryId.HasValue)
        {
            query = query.Where(room => room.category_id == filters.CategoryId.Value);
        }

        if (filters.FloorNumber.HasValue)
        {
            query = query.Where(room => room.floor_number == filters.FloorNumber.Value);
        }

        var filteredRooms = await query
            .OrderBy(room => room.room_number)
            .ToListAsync();

        var allRooms = await _context.rooms
            .AsNoTracking()
            .Include(room => room.status)
            .ToListAsync();

        var model = new RoomIndexViewModel
        {
            Filters = filters,
            Rooms = filteredRooms.Select(MapRoom).ToList(),
            StatusOptions = await BuildStatusOptionsAsync(filters.StatusId),
            CategoryOptions = await BuildCategoryOptionsAsync(filters.CategoryId),
            ActiveFilterSummary = await BuildFilterSummaryAsync(filters),
            Stats = new RoomIndexStatsViewModel
            {
                TotalRooms = allRooms.Count,
                MatchingRooms = filteredRooms.Count,
                AvailableRooms = allRooms.Count(room => string.Equals(room.status?.status_name, "Available", StringComparison.OrdinalIgnoreCase)),
                AverageNightlyRate = allRooms.Count == 0 ? 0 : allRooms.Average(room => room.nightly_rate)
            }
        };

        return View(model);
    }

    public async Task<IActionResult> Details(uint id)
    {
        var room = await LoadRoomGraphAsync(id);
        if (room is null)
        {
            return NotFound();
        }

        return View(BuildDetailsModel(room));
    }

    public async Task<IActionResult> Create()
    {
        return View(await BuildFormModelAsync(new RoomFormViewModel
        {
            PageTitle = "Create Room",
            IntroText = "Capture a polished room record with pricing, placement, and service availability in one flow.",
            SubmitLabel = "Create Room",
            HeroEyebrow = "Inventory Setup"
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoomFormViewModel model)
    {
        if (await RoomNumberExistsAsync(model.RoomNumber))
        {
            ModelState.AddModelError(nameof(model.RoomNumber), $"Room {model.RoomNumber} already exists.");
        }

        if (!ModelState.IsValid)
        {
            model.PageTitle = "Create Room";
            model.IntroText = "Capture a polished room record with pricing, placement, and service availability in one flow.";
            model.SubmitLabel = "Create Room";
            model.HeroEyebrow = "Inventory Setup";
            return View(await BuildFormModelAsync(model));
        }

        var entity = new room
        {
            room_number = model.RoomNumber,
            status_id = model.StatusId,
            category_id = model.CategoryId,
            nightly_rate = model.NightlyRate,
            floor_number = model.FloorNumber
        };

        _context.rooms.Add(entity);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Room {entity.room_number} was created successfully.";
        return RedirectToAction(nameof(Details), new { id = entity.id });
    }

    public async Task<IActionResult> Edit(uint id)
    {
        var room = await _context.rooms.AsNoTracking().FirstOrDefaultAsync(room => room.id == id);
        if (room is null)
        {
            return NotFound();
        }

        return View(await BuildFormModelAsync(new RoomFormViewModel
        {
            Id = room.id,
            RoomNumber = room.room_number,
            StatusId = room.status_id,
            CategoryId = room.category_id,
            NightlyRate = room.nightly_rate,
            FloorNumber = room.floor_number,
            PageTitle = "Edit Room",
            IntroText = "Refine the room profile and keep front-desk, housekeeping, and revenue data aligned.",
            SubmitLabel = "Save Changes",
            HeroEyebrow = "Room Refresh"
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(uint id, RoomFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (await RoomNumberExistsAsync(model.RoomNumber, model.Id))
        {
            ModelState.AddModelError(nameof(model.RoomNumber), $"Room {model.RoomNumber} already exists.");
        }

        if (!ModelState.IsValid)
        {
            model.PageTitle = "Edit Room";
            model.IntroText = "Refine the room profile and keep front-desk, housekeeping, and revenue data aligned.";
            model.SubmitLabel = "Save Changes";
            model.HeroEyebrow = "Room Refresh";
            return View(await BuildFormModelAsync(model));
        }

        var entity = await _context.rooms.FirstOrDefaultAsync(room => room.id == id);
        if (entity is null)
        {
            return NotFound();
        }

        entity.room_number = model.RoomNumber;
        entity.status_id = model.StatusId;
        entity.category_id = model.CategoryId;
        entity.nightly_rate = model.NightlyRate;
        entity.floor_number = model.FloorNumber;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Room {entity.room_number} was updated successfully.";
        return RedirectToAction(nameof(Details), new { id = entity.id });
    }

    public async Task<IActionResult> Delete(uint id)
    {
        var room = await LoadRoomGraphAsync(id);
        if (room is null)
        {
            return NotFound();
        }

        return View(BuildDetailsModel(room));
    }

    [HttpPost, ActionName(nameof(Delete))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(uint id)
    {
        var room = await _context.rooms.FirstOrDefaultAsync(room => room.id == id);
        if (room is null)
        {
            return NotFound();
        }

        _context.rooms.Remove(room);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Room {room.room_number} was deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> GetById(uint id)
    {
        return await Details(id);
    }

    public async Task<IActionResult> GetAll()
    {
        return await Index(new RoomFiltersViewModel());
    }

    public async Task<IActionResult> GetByRoomNumber(uint roomNumber)
    {
        var room = await _roomRepository.GetByRoomNumberAsync(roomNumber);
        if (room is null)
        {
            TempData["ErrorMessage"] = $"Room {roomNumber} was not found.";
            return RedirectToAction(nameof(Index), new { RoomNumber = roomNumber });
        }

        return RedirectToAction(nameof(Details), new { id = room.id });
    }

    public IActionResult GetByNightlyRateRange(decimal minRate, decimal maxRate)
    {
        return RedirectToAction(nameof(Index), new { MinRate = minRate, MaxRate = maxRate });
    }

    public IActionResult GetByStatusId(byte statusId)
    {
        return RedirectToAction(nameof(Index), new { StatusId = statusId });
    }

    public IActionResult GetByCategoryId(byte categoryId)
    {
        return RedirectToAction(nameof(Index), new { CategoryId = categoryId });
    }

    public IActionResult GetByFloorNumber(byte floorNumber)
    {
        return RedirectToAction(nameof(Index), new { FloorNumber = floorNumber });
    }

    public async Task<IActionResult> GetBookingsByRoomId(uint roomId)
    {
        var room = await LoadRoomGraphAsync(roomId);
        if (room is null)
        {
            return NotFound();
        }

        return View("RelatedBookings", new RoomCollectionPageViewModel<RoomBookingViewModel>
        {
            Room = MapRoom(room),
            Title = "Bookings Linked To This Room",
            Description = "Review active and historical reservations tied to this accommodation.",
            Items = room.bookings
                .OrderBy(booking => booking.check_in)
                .Select(MapBooking)
                .ToList()
        });
    }

    public async Task<IActionResult> GetBookingHistoriesByRoomId(uint roomId)
    {
        var room = await LoadRoomGraphAsync(roomId);
        if (room is null)
        {
            return NotFound();
        }

        return View("RelatedHistories", new RoomCollectionPageViewModel<RoomBookingHistoryViewModel>
        {
            Room = MapRoom(room),
            Title = "Booking History Timeline",
            Description = "See every operational change recorded for this room across the guest journey.",
            Items = room.booking_histories
                .OrderByDescending(history => history.created_at)
                .Select(MapBookingHistory)
                .ToList()
        });
    }

    public async Task<IActionResult> GetAvailabilitiesByRoomId(uint roomId)
    {
        var room = await LoadRoomGraphAsync(roomId);
        if (room is null)
        {
            return NotFound();
        }

        return View("RelatedAvailabilities", new RoomCollectionPageViewModel<RoomAvailabilityViewModel>
        {
            Room = MapRoom(room),
            Title = "Availability Windows",
            Description = "Track future availability blocks and scheduling ranges for this room.",
            Items = room.room_availabilities
                .OrderBy(availability => availability.start_schedule)
                .Select(MapAvailability)
                .ToList()
        });
    }

    private async Task<room?> LoadRoomGraphAsync(uint id)
    {
        return await _context.rooms
            .AsNoTracking()
            .Include(room => room.status)
            .Include(room => room.category)
            .Include(room => room.bookings)
                .ThenInclude(booking => booking.customer)
            .Include(room => room.bookings)
                .ThenInclude(booking => booking.status)
            .Include(room => room.booking_histories)
                .ThenInclude(history => history.status)
            .Include(room => room.room_availabilities)
            .FirstOrDefaultAsync(room => room.id == id);
    }

    private async Task<bool> RoomNumberExistsAsync(uint roomNumber, uint? excludedId = null)
    {
        return await _context.rooms.AnyAsync(room =>
            room.room_number == roomNumber &&
            (!excludedId.HasValue || room.id != excludedId.Value));
    }

    private async Task<RoomFormViewModel> BuildFormModelAsync(RoomFormViewModel model)
    {
        model.StatusOptions = await BuildStatusOptionsAsync(model.StatusId);
        model.CategoryOptions = await BuildCategoryOptionsAsync(model.CategoryId);
        return model;
    }

    private async Task<List<SelectListItem>> BuildStatusOptionsAsync(byte? selectedId)
    {
        var statuses = await _roomStatusRepository.GetAllStatusesAsync();
        return statuses
            .OrderBy(status => status.status_name)
            .Select(status => new SelectListItem(status.status_name, status.id.ToString(), status.id == selectedId))
            .ToList();
    }

    private async Task<List<SelectListItem>> BuildCategoryOptionsAsync(byte? selectedId)
    {
        var categories = await _roomCategoryRepository.GetAllCategoriesAsync();
        return categories
            .OrderBy(category => category.category_name)
            .Select(category => new SelectListItem(category.category_name, category.id.ToString(), category.id == selectedId))
            .ToList();
    }

    private static RoomListItemViewModel MapRoom(room room)
    {
        return new RoomListItemViewModel
        {
            Id = room.id,
            RoomNumber = room.room_number,
            FloorNumber = room.floor_number,
            NightlyRate = room.nightly_rate,
            StatusId = room.status_id,
            CategoryId = room.category_id,
            StatusName = room.status?.status_name ?? "Unknown",
            CategoryName = room.category?.category_name ?? "Unknown"
        };
    }

    private static RoomBookingViewModel MapBooking(booking booking)
    {
        return new RoomBookingViewModel
        {
            Id = booking.id,
            ReserveNumber = booking.reserve_number ?? $"BK-{booking.id}",
            CustomerId = booking.customer_id,
            CustomerName = booking.customer is null
                ? $"Guest #{booking.customer_id}"
                : $"{booking.customer.first_name} {booking.customer.last_name}",
            StatusName = booking.status?.status_name ?? "Unknown",
            CheckIn = booking.check_in,
            CheckOut = booking.check_out,
            TotalPrice = booking.total_price
        };
    }

    private static RoomBookingHistoryViewModel MapBookingHistory(booking_history history)
    {
        return new RoomBookingHistoryViewModel
        {
            Id = history.id,
            BookingId = history.booking_id,
            CustomerId = history.customer_id,
            ActionType = history.action_type,
            StatusName = history.status?.status_name ?? "Unknown",
            CheckIn = history.check_in,
            CheckOut = history.check_out,
            CreatedAt = history.created_at,
            TotalPrice = history.total_price
        };
    }

    private static RoomAvailabilityViewModel MapAvailability(room_availability availability)
    {
        return new RoomAvailabilityViewModel
        {
            Id = availability.id,
            StartSchedule = availability.start_schedule,
            EndSchedule = availability.end_schedule,
            CreatedAt = availability.creation_at
        };
    }

    private static RoomDetailsViewModel BuildDetailsModel(room room)
    {
        return new RoomDetailsViewModel
        {
            Room = MapRoom(room),
            Bookings = room.bookings
                .OrderBy(booking => booking.check_in)
                .Select(MapBooking)
                .ToList(),
            BookingHistories = room.booking_histories
                .OrderByDescending(history => history.created_at)
                .Select(MapBookingHistory)
                .ToList(),
            Availabilities = room.room_availabilities
                .OrderBy(availability => availability.start_schedule)
                .Select(MapAvailability)
                .ToList()
        };
    }

    private async Task<string> BuildFilterSummaryAsync(RoomFiltersViewModel filters)
    {
        var segments = new List<string>();

        if (filters.RoomNumber.HasValue)
        {
            segments.Add($"room #{filters.RoomNumber.Value}");
        }

        if (filters.MinRate.HasValue || filters.MaxRate.HasValue)
        {
            var min = filters.MinRate?.ToString("C") ?? "any";
            var max = filters.MaxRate?.ToString("C") ?? "any";
            segments.Add($"rate between {min} and {max}");
        }

        if (filters.StatusId.HasValue)
        {
            var status = await _roomStatusRepository.GetByIdAsync(filters.StatusId.Value);
            segments.Add($"status {status?.status_name ?? filters.StatusId.Value.ToString()}");
        }

        if (filters.CategoryId.HasValue)
        {
            var category = await _roomCategoryRepository.GetByIdAsync(filters.CategoryId.Value);
            segments.Add($"category {category?.category_name ?? filters.CategoryId.Value.ToString()}");
        }

        if (filters.FloorNumber.HasValue)
        {
            segments.Add($"floor {filters.FloorNumber.Value}");
        }

        return segments.Count == 0
            ? "Showing the full room inventory."
            : $"Showing rooms filtered by {string.Join(", ", segments)}.";
    }
}
