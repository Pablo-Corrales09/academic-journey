using HotelCarga.Models.Bookings;
using HotelCarga.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelCarga.Web.Controllers;

public class BookingController : Controller
{
    private const string ApiUnavailableMessage = "Booking service is unavailable. Start HotelCarga.ApiModel and try again.";

    private readonly IBookingApiService _service;
    private readonly ICustomerApiService _customerService;
    private readonly IRoomApiService _roomService;

    public BookingController(IBookingApiService service, ICustomerApiService customerService, IRoomApiService roomService)
    {
        _service = service;
        _customerService = customerService;
        _roomService = roomService;
    }

    public async Task<IActionResult> Index(BookingFiltersViewModel filters)
    {
        List<BookingSummaryDto> allBookings;
        List<CustomerSummaryDto> customers;
        List<RoomDto> rooms;
        List<BookingStatusDto> statuses;

        try
        {
            var bookingsTask = _service.GetAllAsync();
            var customersTask = _customerService.GetAllAsync();
            var roomsTask = _roomService.GetAllAsync();
            var statusesTask = _service.GetStatusesAsync();
            await Task.WhenAll(bookingsTask, customersTask, roomsTask, statusesTask);

            allBookings = bookingsTask.Result;
            customers = customersTask.Result;
            rooms = roomsTask.Result;
            statuses = statusesTask.Result;
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] = ApiUnavailableMessage;
            return View(BuildUnavailableIndexModel(filters));
        }

        var filtered = allBookings.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filters.ReserveNumber))
        {
            filtered = filtered.Where(booking =>
                (booking.reserve_number ?? string.Empty).Contains(filters.ReserveNumber, StringComparison.OrdinalIgnoreCase));
        }

        if (filters.CustomerId.HasValue)
        {
            filtered = filtered.Where(booking => booking.customer_id == filters.CustomerId.Value);
        }

        if (filters.RoomId.HasValue)
        {
            filtered = filtered.Where(booking => booking.room_id == filters.RoomId.Value);
        }

        if (filters.StatusId.HasValue)
        {
            filtered = filtered.Where(booking => booking.status_id == filters.StatusId.Value);
        }

        if (filters.CheckInDate.HasValue)
        {
            filtered = filtered.Where(booking => booking.check_in.Date == filters.CheckInDate.Value.Date);
        }

        var list = filtered
            .OrderByDescending(booking => booking.check_in)
            .Select(MapSummary)
            .ToList();

        return View(new BookingIndexViewModel
        {
            Filters = filters,
            Bookings = list,
            CustomerOptions = BuildCustomerOptions(customers, filters.CustomerId),
            RoomOptions = BuildRoomOptions(rooms, filters.RoomId, null),
            StatusOptions = BuildStatusOptions(statuses, filters.StatusId),
            ActiveFilterSummary = BuildFilterSummary(filters, customers, rooms, statuses),
            Stats = new BookingIndexStatsViewModel
            {
                TotalBookings = allBookings.Count,
                MatchingBookings = list.Count,
                ActiveBookings = allBookings.Count(booking => booking.status_id != 3),
                TotalRevenue = allBookings.Sum(booking => booking.total_price)
            }
        });
    }

    public async Task<IActionResult> Details(uint id)
    {
        try
        {
            var bookingTask = _service.GetByIdAsync(id);
            var statusesTask = _service.GetStatusesAsync();
            await Task.WhenAll(bookingTask, statusesTask);

            var booking = bookingTask.Result;
            if (booking is null)
            {
                return NotFound();
            }

            var historyIds = await _service.GetBookingHistoryIdsByReserveNumberAsync(booking.reserve_number ?? string.Empty);
            var statusDescription = statusesTask.Result.FirstOrDefault(status => status.id == booking.status_id)?.description
                ?? "No status description available.";

            return View(new BookingDetailsViewModel
            {
                Booking = MapSummary(booking),
                BookingHistoryIds = historyIds,
                StatusDescription = statusDescription,
                StayLengthNights = Math.Max(1, (booking.check_out.Date - booking.check_in.Date).Days)
            });
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] = ApiUnavailableMessage;
            return RedirectToAction(nameof(Index));
        }
    }

    public IActionResult GetAll()
    {
        return RedirectToAction(nameof(Index));
    }

    public IActionResult GetById(uint id)
    {
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Create()
    {
        try
        {
            return View(await BuildFormModelAsync(new BookingFormViewModel
            {
                CheckIn = DateTime.Today,
                CheckOut = DateTime.Today.AddDays(1),
                LockCustomerSelection = false,
                PageTitle = "Create Booking",
                IntroText = "Open a reservation with guest, room, and stay dates while the API calculates reserve number, status, and pricing.",
                SubmitLabel = "Create Booking",
                HeroEyebrow = "Reservation Intake"
            }));
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingFormViewModel model)
    {
        try
        {
            ValidateDates(model);

            if (!ModelState.IsValid)
            {
                // Debug: Add model state errors to TempData for visibility
                var errorSummary = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["ValidationErrors"] = errorSummary;
                
                model.PageTitle = "Create Booking";
                model.IntroText = "Open a reservation with guest, room, and stay dates while the API calculates reserve number, status, and pricing.";
                model.SubmitLabel = "Create Booking";
                model.HeroEyebrow = "Reservation Intake";
                model.LockCustomerSelection = false;
                return View(await BuildFormModelAsync(model));
            }

            var id = await _service.CreateAsync(new SaveBookingDto(model.CustomerId, model.RoomId, model.CheckIn, model.CheckOut));
            TempData["SuccessMessage"] = "Booking created successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (BookingApiException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.PageTitle = "Create Booking";
            model.IntroText = "Open a reservation with guest, room, and stay dates while the API calculates reserve number, status, and pricing.";
            model.SubmitLabel = "Create Booking";
            model.HeroEyebrow = "Reservation Intake";
            model.LockCustomerSelection = false;
            return View(await BuildFormModelAsync(model));
        }
        catch (HttpRequestException ex)
        {
            TempData["ErrorMessage"] = $"API Error: {ex.Message}";
            return RedirectToIndexWithApiError();
        }
    }

    public async Task<IActionResult> Update(uint id)
    {
        try
        {
            var booking = await _service.GetByIdAsync(id);
            if (booking is null)
            {
                return NotFound();
            }

            return View(await BuildFormModelAsync(new BookingFormViewModel
            {
                Id = booking.id,
                CustomerId = booking.customer_id,
                RoomId = booking.room_id,
                CheckIn = booking.check_in,
                CheckOut = booking.check_out,
                CurrentReserveNumber = booking.reserve_number ?? string.Empty,
                CurrentStatusName = booking.status_name,
                CurrentNightlyRate = booking.nightly_rate,
                CurrentTotalPrice = booking.total_price,
                LockCustomerSelection = true,
                PageTitle = "Update Booking",
                IntroText = "Adjust stay dates or room assignment while the API enforces room availability and recalculates totals.",
                SubmitLabel = "Save Changes",
                HeroEyebrow = "Reservation Update"
            }));
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(uint id, BookingFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        try
        {
            ValidateDates(model);

            if (!ModelState.IsValid)
            {
                model.PageTitle = "Update Booking";
                model.IntroText = "Adjust stay dates or room assignment while the API enforces room availability and recalculates totals.";
                model.SubmitLabel = "Save Changes";
                model.HeroEyebrow = "Reservation Update";
                model.LockCustomerSelection = true;
                return View(await BuildFormModelAsync(model));
            }

            var existing = await _service.GetByIdAsync(id);
            if (existing is null)
            {
                return NotFound();
            }

            await _service.UpdateAsync(id, new SaveBookingDto(existing.customer_id, model.RoomId, model.CheckIn, model.CheckOut, existing.status_id));
            TempData["SuccessMessage"] = "Booking updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (BookingApiException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.PageTitle = "Update Booking";
            model.IntroText = "Adjust stay dates or room assignment while the API enforces room availability and recalculates totals.";
            model.SubmitLabel = "Save Changes";
            model.HeroEyebrow = "Reservation Update";
            model.LockCustomerSelection = true;
            return View(await BuildFormModelAsync(model));
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    public async Task<IActionResult> Delete(uint id)
    {
        try
        {
            var booking = await _service.GetByIdAsync(id);
            if (booking is null)
            {
                return NotFound();
            }

            return View(MapSummary(booking));
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(uint id)
    {
        try
        {
            await _service.DeleteAsync(id);
            TempData["SuccessMessage"] = "Booking cancelled successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (BookingApiException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    public async Task<IActionResult> GetReserveNumberById(uint id)
    {
        try
        {
            var reserveNumber = await _service.GetReserveNumberByIdAsync(id);
            return View("Lookup", new BookingLookupResultViewModel
            {
                Title = "Reserve Number by Id",
                Description = "Resolve the reservation code for a booking id.",
                QueryLabel = "Booking Id",
                QueryValue = id.ToString(),
                ResultLabel = "Reserve Number",
                ScalarResult = reserveNumber,
                NotFoundMessage = string.IsNullOrWhiteSpace(reserveNumber) ? "No reserve number was returned for this booking id." : null
            });
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    public async Task<IActionResult> GetRoomIdByReserveNumber(string reserveNumber)
    {
        try
        {
            var roomId = await _service.GetRoomIdByReserveNumberAsync(reserveNumber);
            return View("Lookup", new BookingLookupResultViewModel
            {
                Title = "Room Id by Reserve Number",
                Description = "Resolve the assigned room id from a reservation code.",
                QueryLabel = "Reserve Number",
                QueryValue = reserveNumber,
                ResultLabel = "Room Id",
                UIntResult = roomId,
                NotFoundMessage = roomId is null ? "No room id was returned for this reserve number." : null
            });
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    public async Task<IActionResult> GetBookingHistoryIdsByReserveNumber(string reserveNumber)
    {
        try
        {
            var ids = await _service.GetBookingHistoryIdsByReserveNumberAsync(reserveNumber);
            return View("Lookup", new BookingLookupResultViewModel
            {
                Title = "Booking History Ids by Reserve Number",
                Description = "List booking history ids linked to a reserve number.",
                QueryLabel = "Reserve Number",
                QueryValue = reserveNumber,
                ResultLabel = "Booking History Ids",
                IdListResult = ids,
                NotFoundMessage = ids.Count == 0 ? "No booking history ids were returned for this reserve number." : null
            });
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    public async Task<IActionResult> GetOverlappingReserveNumbers(DateTime startDate, DateTime endDate)
    {
        try
        {
            var reserveNumbers = await _service.GetOverlappingReserveNumbersAsync(startDate, endDate);
            return View("Lookup", new BookingLookupResultViewModel
            {
                Title = "Overlapping Reserve Numbers",
                Description = "Find reservations whose stay range overlaps the requested date window.",
                QueryLabel = "Date Window",
                QueryValue = $"{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
                ResultLabel = "Reserve Numbers",
                ReserveNumbers = reserveNumbers,
                NotFoundMessage = reserveNumbers.Count == 0 ? "No overlapping reserve numbers were returned." : null
            });
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    public async Task<IActionResult> GetReserveNumbersByCheckInDate(DateTime checkInDate)
    {
        try
        {
            var reserveNumbers = await _service.GetReserveNumbersByCheckInDateAsync(checkInDate);
            return View("Lookup", new BookingLookupResultViewModel
            {
                Title = "Reserve Numbers by Check-In Date",
                Description = "List reserve numbers scheduled to begin on a specific date.",
                QueryLabel = "Check-In Date",
                QueryValue = checkInDate.ToString("yyyy-MM-dd"),
                ResultLabel = "Reserve Numbers",
                ReserveNumbers = reserveNumbers,
                NotFoundMessage = reserveNumbers.Count == 0 ? "No reserve numbers were returned for that check-in date." : null
            });
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    public async Task<IActionResult> GetTotalPriceByReserveNumber(string reserveNumber)
    {
        try
        {
            var totalPrice = await _service.GetTotalPriceByReserveNumberAsync(reserveNumber);
            return View("Lookup", new BookingLookupResultViewModel
            {
                Title = "Total Price by Reserve Number",
                Description = "Resolve the calculated booking total for a reservation code.",
                QueryLabel = "Reserve Number",
                QueryValue = reserveNumber,
                ResultLabel = "Total Price",
                DecimalResult = totalPrice,
                NotFoundMessage = totalPrice is null ? "No total price was returned for this reserve number." : null
            });
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    public async Task<IActionResult> GetReserveNumbersByStatusId(byte statusId)
    {
        try
        {
            var reserveNumbers = await _service.GetReserveNumbersByStatusIdAsync(statusId);
            return View("Lookup", new BookingLookupResultViewModel
            {
                Title = "Reserve Numbers by Status",
                Description = "List reserve numbers that match the selected booking status.",
                QueryLabel = "Status Id",
                QueryValue = statusId.ToString(),
                ResultLabel = "Reserve Numbers",
                ReserveNumbers = reserveNumbers,
                NotFoundMessage = reserveNumbers.Count == 0 ? "No reserve numbers were returned for that status." : null
            });
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    public async Task<IActionResult> GetReserveNumbersByCustomerId(uint customerId)
    {
        try
        {
            var reserveNumbers = await _service.GetReserveNumbersByCustomerIdAsync(customerId);
            return View("Lookup", new BookingLookupResultViewModel
            {
                Title = "Reserve Numbers by Customer",
                Description = "List reserve numbers assigned to the selected customer.",
                QueryLabel = "Customer Id",
                QueryValue = customerId.ToString(),
                ResultLabel = "Reserve Numbers",
                ReserveNumbers = reserveNumbers,
                NotFoundMessage = reserveNumbers.Count == 0 ? "No reserve numbers were returned for that customer." : null
            });
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    private async Task<BookingFormViewModel> BuildFormModelAsync(BookingFormViewModel model)
    {
        var customersTask = _customerService.GetAllAsync();
        var roomsTask = _roomService.GetAllAsync();
        await Task.WhenAll(customersTask, roomsTask);

        model.CustomerOptions = BuildCustomerOptions(customersTask.Result, model.CustomerId);
        model.RoomOptions = BuildRoomOptions(roomsTask.Result, model.RoomId, model.RoomId == 0 ? null : model.RoomId);
        return model;
    }

    private IActionResult RedirectToIndexWithApiError()
    {
        TempData["ErrorMessage"] = ApiUnavailableMessage;
        return RedirectToAction(nameof(Index));
    }

    private static BookingIndexViewModel BuildUnavailableIndexModel(BookingFiltersViewModel filters)
    {
        return new BookingIndexViewModel
        {
            Filters = filters,
            Bookings = [],
            CustomerOptions = [],
            RoomOptions = [],
            StatusOptions = [],
            ActiveFilterSummary = "Booking API is offline.",
            Stats = new BookingIndexStatsViewModel()
        };
    }

    private static List<SelectListItem> BuildCustomerOptions(IEnumerable<CustomerSummaryDto> customers, uint? selectedId)
    {
        return customers
            .OrderBy(customer => customer.last_name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(customer => customer.first_name, StringComparer.OrdinalIgnoreCase)
            .Select(customer => new SelectListItem(
                $"{customer.first_name} {customer.last_name} - {customer.document_number}",
                customer.id.ToString(),
                customer.id == selectedId))
            .ToList();
    }

    private static List<SelectListItem> BuildRoomOptions(IEnumerable<RoomDto> rooms, uint? selectedId, uint? currentRoomId)
    {
        return rooms
            .Where(room => room.status_id == 1 || room.id == currentRoomId)
            .OrderBy(room => room.room_number)
            .Select(room => new SelectListItem(
                $"Room {room.room_number} - {(room.status_name ?? "Unknown")}",
                room.id.ToString(),
                room.id == selectedId))
            .ToList();
    }

    private static List<SelectListItem> BuildStatusOptions(IEnumerable<BookingStatusDto> statuses, byte? selectedId)
    {
        return statuses
            .OrderBy(status => status.status_name, StringComparer.OrdinalIgnoreCase)
            .Select(status => new SelectListItem(status.status_name, status.id.ToString(), status.id == selectedId))
            .ToList();
    }

    private static BookingListItemViewModel MapSummary(BookingSummaryDto booking)
    {
        return new BookingListItemViewModel
        {
            Id = booking.id,
            ReserveNumber = booking.reserve_number ?? "Pending",
            StatusId = booking.status_id,
            StatusName = booking.status_name,
            CustomerId = booking.customer_id,
            CustomerName = booking.customer_name,
            RoomId = booking.room_id,
            RoomNumber = booking.room_number,
            CheckIn = booking.check_in,
            CheckOut = booking.check_out,
            NightlyRate = booking.nightly_rate,
            TotalPrice = booking.total_price
        };
    }

    private static string BuildFilterSummary(
        BookingFiltersViewModel filters,
        IEnumerable<CustomerSummaryDto> customers,
        IEnumerable<RoomDto> rooms,
        IEnumerable<BookingStatusDto> statuses)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(filters.ReserveNumber))
        {
            parts.Add($"reserve number contains '{filters.ReserveNumber}'");
        }

        if (filters.CustomerId.HasValue)
        {
            var customer = customers.FirstOrDefault(item => item.id == filters.CustomerId.Value);
            if (customer is not null)
            {
                parts.Add($"customer is {customer.first_name} {customer.last_name}");
            }
        }

        if (filters.RoomId.HasValue)
        {
            var room = rooms.FirstOrDefault(item => item.id == filters.RoomId.Value);
            if (room is not null)
            {
                parts.Add($"room is {room.room_number}");
            }
        }

        if (filters.StatusId.HasValue)
        {
            var status = statuses.FirstOrDefault(item => item.id == filters.StatusId.Value);
            if (status is not null)
            {
                parts.Add($"status is {status.status_name}");
            }
        }

        if (filters.CheckInDate.HasValue)
        {
            parts.Add($"check-in date is {filters.CheckInDate.Value:yyyy-MM-dd}");
        }

        return parts.Count == 0 ? "Showing the full booking ledger." : $"Filtered by {string.Join(", ", parts)}.";
    }

    private void ValidateDates(BookingFormViewModel model)
    {
        if (model.CheckIn.Date < DateTime.Today)
        {
            ModelState.AddModelError(nameof(model.CheckIn), "Check-in date cannot be earlier than today.");
        }

        if (model.CheckOut.Date < model.CheckIn.Date)
        {
            ModelState.AddModelError(nameof(model.CheckOut), "Check-out date cannot be earlier than check-in date.");
        }
    }
}