using HotelCarga.Models.Rooms;
using HotelCarga.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http;
using System.Linq;

namespace HotelCarga.Web.Controllers;

public class RoomController : Controller
{
    private readonly IRoomApiService _service;
    private const string ApiUnavailableMessage = "Room service is unavailable. Start HotelCarga.ApiModel and try again.";


    public RoomController(IRoomApiService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index(RoomFiltersViewModel filters)
    {
        List<RoomDto> allRooms;
        try
        {
            allRooms = await _service.GetAllAsync();
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] = ApiUnavailableMessage;
            return View(BuildUnavailableIndexModel(filters));
        }

        var filteredRooms = allRooms.AsQueryable();

        if (filters.RoomNumber.HasValue)
        {
            filteredRooms = filteredRooms.Where(r => r.room_number == filters.RoomNumber.Value);
        }

        if (filters.MinRate.HasValue)
        {
            filteredRooms = filteredRooms.Where(r => r.nightly_rate >= filters.MinRate.Value);
        }

        if (filters.MaxRate.HasValue)
        {
            filteredRooms = filteredRooms.Where(r => r.nightly_rate <= filters.MaxRate.Value);
        }

        if (filters.StatusId.HasValue)
        {
            filteredRooms = filteredRooms.Where(r => r.status_id == filters.StatusId.Value);
        }

        if (filters.CategoryId.HasValue)
        {
            filteredRooms = filteredRooms.Where(r => r.category_id == filters.CategoryId.Value);
        }

        if (filters.FloorNumber.HasValue)
        {
            filteredRooms = filteredRooms.Where(r => r.floor_number == filters.FloorNumber.Value);
        }

        var filteredList = filteredRooms.OrderBy(r => r.room_number).ToList();

        var model = new RoomIndexViewModel
        {
            Filters = filters,
            Rooms = filteredList.Select(MapRoomDto).ToList(),
            StatusOptions = await BuildStatusOptionsAsync(filters.StatusId),
            CategoryOptions = await BuildCategoryOptionsAsync(filters.CategoryId),
            ActiveFilterSummary = await BuildFilterSummaryAsync(filters),
            Stats = new RoomIndexStatsViewModel
            {
                TotalRooms = allRooms.Count,
                MatchingRooms = filteredList.Count,
                AvailableRooms = allRooms.Count(r => string.Equals(r.status_name, "Available", StringComparison.OrdinalIgnoreCase)),
                AverageNightlyRate = allRooms.Count == 0 ? 0 : allRooms.Average(r => r.nightly_rate)
            }
        };

        return View(model);
    }

    public async Task<IActionResult> Details(uint id)
    {
        RoomDto? roomDto;
        try
        {
            roomDto = await _service.GetByIdAsync(id);
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] = ApiUnavailableMessage;
            return RedirectToAction(nameof(Index));
        }

        if (roomDto is null)
        {
            return NotFound();
        }

        List<BookingDto> bookings;
        List<BookingHistoryDto> histories;
        List<RoomAvailabilityDto> availabilities;

        try
        {
            bookings = await _service.GetBookingsByRoomIdAsync(id);
            histories = await _service.GetBookingHistoriesByRoomIdAsync(id);
            availabilities = await _service.GetAvailabilitiesByRoomIdAsync(id);
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] = ApiUnavailableMessage;
            return RedirectToAction(nameof(Index));
        }

        return View(new RoomDetailsViewModel
        {
            Room = MapRoomDto(roomDto),
            Bookings = bookings.Select(MapBookingDto).OrderBy(b => b.CheckIn).ToList(),
            BookingHistories = histories.Select(MapBookingHistoryDto).OrderByDescending(h => h.CreatedAt).ToList(),
            Availabilities = availabilities.Select(MapAvailabilityDto).OrderBy(a => a.StartSchedule).ToList()
        });
    }

    public async Task<IActionResult> Create()
    {
        try
        {
            return View(await BuildFormModelAsync(new RoomFormViewModel
            {
                PageTitle = "Create Room",
                IntroText = "Capture a polished room record with pricing, placement, and service availability in one flow.",
                SubmitLabel = "Create Room",
                HeroEyebrow = "Inventory Setup"
            }));
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoomFormViewModel model)
    {
        try
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

            var dto = new CreateRoomDto(model.RoomNumber, model.StatusId, model.CategoryId, model.NightlyRate, model.FloorNumber);

            var newRoom = await _service.CreateAsync(dto);

            TempData["SuccessMessage"] = $"Room {newRoom.room_number} was created successfully.";
            return RedirectToAction(nameof(Details), new { id = newRoom.id });
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    public async Task<IActionResult> Edit(uint id)
    {
        RoomDto? room;
        try
        {
            room = await _service.GetByIdAsync(id);
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }

        if (room is null)
        {
            return NotFound();
        }

        try
        {
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
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(uint id, RoomFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        try
        {
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

            var dto = new RoomDto(id, model.RoomNumber, model.FloorNumber, model.NightlyRate, model.StatusId, model.CategoryId, null, null);
            var updatedRoom = await _service.UpdateAsync(dto);

            TempData["SuccessMessage"] = $"Room {updatedRoom.room_number} was updated successfully.";
            return RedirectToAction(nameof(Details), new { id = updatedRoom.id });
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    public async Task<IActionResult> Delete(uint id)
    {
        RoomDto? roomDto;
        try
        {
            roomDto = await _service.GetByIdAsync(id);
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }

        if (roomDto is null)
        {
            return NotFound();
        }

        return View(MapRoomDto(roomDto));
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(uint id)
    {
        try
        {
            await _service.DeleteAsync(id);
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }

        TempData["SuccessMessage"] = $"Room deleted successfully.";
        return RedirectToAction(nameof(Index));
    }



    public IActionResult GetAll()
    {
        return RedirectToAction(nameof(Index));
    }

    // Removed GetByRoomNumber - use Index filter or service
    public IActionResult GetByRoomNumber(uint roomNumber)
    {
        return RedirectToAction(nameof(Index), new { RoomNumber = roomNumber });
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


    // Redirect to Details for simplicity
    public async Task<IActionResult> GetBookingHistoriesByRoomId(uint roomId)
    {
        return RedirectToAction(nameof(Details), new { id = roomId });
    }

    // Redirect to Details for simplicity
    public async Task<IActionResult> GetAvailabilitiesByRoomId(uint roomId)
    {
        return RedirectToAction(nameof(Details), new { id = roomId });
    }



    private async Task<bool> RoomNumberExistsAsync(uint roomNumber, uint? excludedId = null)
    {
        return await _service.RoomNumberExistsAsync(roomNumber, excludedId);
    }

    private static RoomIndexViewModel BuildUnavailableIndexModel(RoomFiltersViewModel filters)
    {
        return new RoomIndexViewModel
        {
            Filters = filters,
            Rooms = new List<RoomListItemViewModel>(),
            StatusOptions = new List<SelectListItem>(),
            CategoryOptions = new List<SelectListItem>(),
            ActiveFilterSummary = "Room API is offline.",
            Stats = new RoomIndexStatsViewModel
            {
                TotalRooms = 0,
                MatchingRooms = 0,
                AvailableRooms = 0,
                AverageNightlyRate = 0
            }
        };
    }

    private IActionResult RedirectToIndexWithApiError()
    {
        TempData["ErrorMessage"] = ApiUnavailableMessage;
        return RedirectToAction(nameof(Index));
    }

    private async Task<RoomFormViewModel> BuildFormModelAsync(RoomFormViewModel model)
    {
        model.StatusOptions = await BuildStatusOptionsAsync(model.StatusId);
        model.CategoryOptions = await BuildCategoryOptionsAsync(model.CategoryId);
        return model;
    }

    private async Task<List<SelectListItem>> BuildStatusOptionsAsync(byte? selectedId)
    {
        var allRooms = await _service.GetAllAsync();
        var statuses = allRooms
            .GroupBy(r => new { r.status_id, r.status_name })
            .OrderBy(g => g.Key.status_name)
            .Select(g => new SelectListItem(g.Key.status_name ?? "Unknown", g.Key.status_id.ToString(), g.Key.status_id == selectedId));
        return statuses.ToList();
    }

    private async Task<List<SelectListItem>> BuildCategoryOptionsAsync(byte? selectedId)
    {
        var allRooms = await _service.GetAllAsync();
        var categories = allRooms
            .GroupBy(r => new { r.category_id, r.category_name })
            .OrderBy(g => g.Key.category_name)
            .Select(g => new SelectListItem(g.Key.category_name ?? "Unknown", g.Key.category_id.ToString(), g.Key.category_id == selectedId));
        return categories.ToList();
    }

    private static RoomListItemViewModel MapRoomDto(RoomDto room)
    {
        return new RoomListItemViewModel
        {
            Id = room.id,
            RoomNumber = room.room_number,
            FloorNumber = room.floor_number,
            NightlyRate = room.nightly_rate,
            StatusId = room.status_id,
            CategoryId = room.category_id,
            StatusName = room.status_name ?? "Unknown",
            CategoryName = room.category_name ?? "Unknown"
        };
    }

    private static RoomBookingViewModel MapBookingDto(BookingDto dto)
    {
        return new RoomBookingViewModel
        {
            Id = dto.id,
            ReserveNumber = dto.reserveNumber,
            CustomerId = dto.customerId,
            CustomerName = dto.customerName,
            StatusName = dto.statusName,
            CheckIn = dto.checkIn,
            CheckOut = dto.checkOut,
            TotalPrice = dto.totalPrice
        };
    }

    private static RoomBookingHistoryViewModel MapBookingHistoryDto(BookingHistoryDto dto)
    {
        return new RoomBookingHistoryViewModel
        {
            Id = dto.id,
            BookingId = dto.bookingId,
            CustomerId = dto.customerId,
            ActionType = dto.actionType,
            StatusName = dto.statusName,
            CheckIn = dto.checkIn,
            CheckOut = dto.checkOut,
            CreatedAt = dto.createdAt,
            TotalPrice = dto.totalPrice
        };
    }

    private static RoomAvailabilityViewModel MapAvailabilityDto(RoomAvailabilityDto dto)
    {
        return new RoomAvailabilityViewModel
        {
            Id = dto.id,
            StartSchedule = dto.startSchedule,
            EndSchedule = dto.endSchedule,
            CreatedAt = dto.createdAt
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
            segments.Add($"status ID {filters.StatusId.Value}");
        }

        if (filters.CategoryId.HasValue)
        {
            segments.Add($"category ID {filters.CategoryId.Value}");
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
