using HotelCarga.Models.Bookings;
using HotelCarga.Models.WaitingQueue;
using HotelCarga.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace HotelCarga.Web.Controllers;

public class BookingController : Controller
{
    private const string ApiUnavailableMessage = "Booking service is unavailable. Start HotelCarga.ApiModel and try again.";
    private const string NoAvailabilityPrompt = "Habitación no disponible en el horario seleccionado ¿gusta añadirlo a la cola de solicitudes?";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IBookingApiService _service;
    private readonly ICustomerApiService _customerService;
    private readonly IRoomApiService _roomService;
    private readonly HttpClient _httpClient;

    public BookingController(
        IBookingApiService service,
        ICustomerApiService customerService,
        IRoomApiService roomService,
        IHttpClientFactory clientFactory,
        IOptions<ApiSettings> settings)
    {
        _service = service;
        _customerService = customerService;
        _roomService = roomService;
        _httpClient = clientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(settings.Value.BaseUrl);
    }

    public async Task<IActionResult> Index(string? search = null)
    {
        try
        {
            var rooms = await _roomService.GetAllAsync();
            var roomCards = rooms
                .OrderBy(room => room.room_number)
                .Select(room => new BookingRoomListItemViewModel
                {
                    Id = room.id,
                    RoomNumber = room.room_number,
                    FloorNumber = room.floor_number,
                    NightlyRate = room.nightly_rate,
                    StatusName = string.IsNullOrWhiteSpace(room.status_name) ? "Unknown" : room.status_name,
                    CategoryName = string.IsNullOrWhiteSpace(room.category_name) ? "Room" : room.category_name
                })
                .ToList();

            var roomTypes = rooms
                .Where(room => room.status_id == 1)
                .GroupBy(room => room.category_name ?? "Standard")
                .Select(group => new BookingRoomTypeCardViewModel
                {
                    CategoryName = group.Key,
                    Description = BuildRoomTypeDescription(group.Key),
                    NightlyRateFrom = group.Min(room => room.nightly_rate),
                    NightlyRateTo = group.Max(room => room.nightly_rate),
                    AvailableRooms = group.Count()
                })
                .OrderBy(card => card.CategoryName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return View(new BookingIndexViewModel
            {
                Rooms = roomCards,
                RoomTypes = roomTypes,
                SearchTerm = search,
                IsApiAvailable = true
            });
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] = ApiUnavailableMessage;
            return View(new BookingIndexViewModel
            {
                IsApiAvailable = false,
                ActiveFilterSummary = "Booking API is offline."
            });
        }
    }

    public async Task<IActionResult> List(BookingFiltersViewModel filters)
    {
        List<BookingSummaryDto> allBookings;
        List<WaitingQueueApiDto> queueEntries;
        List<CustomerSummaryDto> customers;
        List<RoomDto> rooms;
        List<BookingStatusDto> statuses;

        try
        {
            var bookingsTask = _service.GetAllAsync();
            var queueTask = FetchPendingQueueEntriesAsync();
            var customersTask = _customerService.GetAllAsync();
            var roomsTask = _roomService.GetAllAsync();
            var statusesTask = _service.GetStatusesAsync();
            await Task.WhenAll(bookingsTask, queueTask, customersTask, roomsTask, statusesTask);

            allBookings = bookingsTask.Result;
            queueEntries = queueTask.Result;
            customers = customersTask.Result;
            rooms = roomsTask.Result;
            statuses = statusesTask.Result;
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] = ApiUnavailableMessage;
            return View(new BookingIndexViewModel
            {
                Filters = filters,
                Bookings = [],
                CustomerOptions = [],
                RoomOptions = [],
                StatusOptions = [],
                ActiveFilterSummary = "Booking API is offline.",
                Stats = new BookingIndexStatsViewModel(),
                IsApiAvailable = false
            });
        }

        if (User.IsInRole("CUSTOMER"))
        {
            var currentUsername = User.Identity?.Name;
            var linkedCustomer = customers.FirstOrDefault(customer =>
                !string.IsNullOrWhiteSpace(customer.username) &&
                string.Equals(customer.username, currentUsername, StringComparison.OrdinalIgnoreCase));

            allBookings = linkedCustomer is null
                ? []
                : allBookings.Where(booking => booking.customer_id == linkedCustomer.id).ToList();

            queueEntries = linkedCustomer is null
                ? []
                : queueEntries.Where(entry => entry.customer_id == linkedCustomer.id).ToList();

            filters.CustomerId = linkedCustomer?.id;
        }
        else if (!User.IsInRole("ADMIN"))
        {
            allBookings = [];
            queueEntries = [];
        }

        var allEntries = allBookings.Select(MapSummary).ToList();
        allEntries.AddRange(MapQueueEntries(queueEntries, allBookings, rooms));

        var filtered = allEntries.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            filtered = filtered.Where(booking =>
                (booking.ReserveNumber ?? string.Empty).Contains(filters.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                (booking.CustomerName ?? string.Empty).Contains(filters.SearchTerm, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filters.ReserveNumber))
        {
            filtered = filtered.Where(booking =>
                (booking.ReserveNumber ?? string.Empty).Contains(filters.ReserveNumber, StringComparison.OrdinalIgnoreCase));
        }

        if (filters.CustomerId.HasValue)
        {
            filtered = filtered.Where(booking => booking.CustomerId == filters.CustomerId.Value);
        }

        if (filters.RoomId.HasValue)
        {
            filtered = filtered.Where(booking => booking.RoomId == filters.RoomId.Value);
        }

        if (filters.StatusId.HasValue)
        {
            filtered = filtered.Where(booking => booking.StatusId == filters.StatusId.Value);
        }

        if (filters.CheckInDate.HasValue)
        {
            filtered = filtered.Where(booking => booking.CheckIn.Date == filters.CheckInDate.Value.Date);
        }

        var ordered = filtered
            .OrderByDescending(booking => booking.CheckIn)
            .ThenByDescending(booking => booking.Id);

        var totalMatching = ordered.Count();
        var pageSize = filters.PageSize <= 0 ? 12 : Math.Min(filters.PageSize, 100);
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalMatching / (double)pageSize));
        var page = filters.Page <= 0 ? 1 : Math.Min(filters.Page, totalPages);

        filters.Page = page;
        filters.PageSize = pageSize;

        var list = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return View("List", new BookingIndexViewModel
        {
            Filters = filters,
            Bookings = list,
            CustomerOptions = BuildCustomerOptions(customers, filters.CustomerId),
            RoomOptions = BuildRoomOptions(rooms, filters.RoomId, null),
            StatusOptions = BuildStatusOptions(statuses, filters.StatusId),
            ActiveFilterSummary = BuildFilterSummary(filters, customers, rooms, statuses),
            Stats = new BookingIndexStatsViewModel
            {
                TotalBookings = allEntries.Count,
                MatchingBookings = totalMatching,
                ActiveBookings = allEntries.Count(booking => booking.StatusId != 3),
                TotalRevenue = allEntries.Sum(booking => booking.TotalPrice)
            },
            Pagination = new BookingPaginationViewModel
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalMatching,
                TotalPages = totalPages
            },
            IsApiAvailable = true
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
        return RedirectToAction(nameof(List));
    }

    public IActionResult GetById(uint id)
    {
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> BookingCreate(
        uint? roomId = null,
        uint? roomNumber = null,
        string? roomType = null,
        uint? customerId = null,
        string? customerCreated = null)
    {
        var result = await Create(customerId, customerCreated, roomId, roomNumber, roomType);
        if (result is ViewResult viewResult && string.IsNullOrWhiteSpace(viewResult.ViewName))
        {
            viewResult.ViewName = "Create";
        }

        return result;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BookingCreate(BookingFormViewModel model, bool addToQueueIfUnavailable = false)
    {
        var result = await Create(model, addToQueueIfUnavailable);
        if (result is ViewResult viewResult && string.IsNullOrWhiteSpace(viewResult.ViewName))
        {
            viewResult.ViewName = "Create";
        }

        return result;
    }

    [HttpGet]
    public async Task<IActionResult> Create(
        uint? customerId = null,
        string? customerCreated = null,
        uint? roomId = null,
        uint? roomNumber = null,
        string? roomType = null)
    {
        try
        {
            if (customerCreated == "1")
            {
                TempData["SuccessMessage"] = "Cliente agregado. Ahora puedes continuar con la reserva.";
            }

            uint effectiveCustomerId = customerId ?? 0;
            if (User.IsInRole("CUSTOMER"))
            {
                var currentUsername = User.Identity?.Name;
                var allCustomers = await _customerService.GetAllAsync();
                var linked = allCustomers.FirstOrDefault(c =>
                    !string.IsNullOrWhiteSpace(c.username) &&
                    string.Equals(c.username, currentUsername, StringComparison.OrdinalIgnoreCase));
                effectiveCustomerId = linked?.id ?? 0;
            }

            var effectiveRoomId = await ResolveRoomSelectionAsync(roomId, roomNumber, roomType);

            return View(await BuildFormModelAsync(new BookingFormViewModel
            {
                CustomerId = effectiveCustomerId,
                RoomId = effectiveRoomId,
                CheckIn = DateTime.Today,
                CheckOut = DateTime.Today.AddDays(1),
                LockCustomerSelection = false,
                PageTitle = "Registro de reservas",
                IntroText = "Gestione sus reservas facilmente desde una sola ventana",
                SubmitLabel = "Crear reserva",
                HeroEyebrow = "Registro de reservas",
                AddCustomerReturnUrl = Url.Action(nameof(Create), "Booking")
            }));
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    [HttpGet]
    public IActionResult StartAddCustomerFlow()
    {
        var bookingReturnUrl = Url.Action(nameof(Create), "Booking") ?? "/Booking/Create";
        return RedirectToAction("Create", "User", new
        {
            createCustomerAfter = true,
            bookingReturnUrl
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingFormViewModel model, bool addToQueueIfUnavailable = false)
    {
        bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        try
        {
            // CUSTOMER role: auto-assign the logged-in user's customer record
            if (User.IsInRole("CUSTOMER"))
            {
                var currentUsername = User.Identity?.Name;
                var allCustomers = await _customerService.GetAllAsync();
                var linkedCustomer = allCustomers.FirstOrDefault(c =>
                    !string.IsNullOrWhiteSpace(c.username) &&
                    string.Equals(c.username, currentUsername, StringComparison.OrdinalIgnoreCase));

                if (linkedCustomer is null)
                {
                    const string noProfileError = "No hay un perfil de cliente vinculado a su cuenta.";
                    if (isAjax) return Json(new { result_type = "VALIDATION_ERROR", success = false, message = noProfileError });
                    ModelState.AddModelError(string.Empty, noProfileError);
                    model.PageTitle = "Gestión de reservas";
                    model.IntroText = "AIngrese los detalles de la estadía y deje que el sistema se encargue del resto.";
                    model.SubmitLabel = "Crear reserva";
                    model.HeroEyebrow = "Registro de reservas";
                    model.LockCustomerSelection = false;
                    model.AddCustomerReturnUrl = Url.Action(nameof(Create), "Booking");
                    return View(await BuildFormModelAsync(model));
                }

                model.CustomerId = linkedCustomer.id;
                ModelState.Remove(nameof(BookingFormViewModel.CustomerId));
            }

            ValidateDates(model);

            if (!ModelState.IsValid)
            {
                var errorSummary = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                if (isAjax) return Json(new { result_type = "VALIDATION_ERROR", success = false, message = errorSummary });
                TempData["ValidationErrors"] = errorSummary;
                model.PageTitle = "Create Booking";
                model.IntroText = "Open a reservation with guest, room, and stay dates while the API calculates reserve number, status, and pricing.";
                model.SubmitLabel = "Create Booking";
                model.HeroEyebrow = "Reservation Intake";
                model.LockCustomerSelection = false;
                model.AddCustomerReturnUrl = Url.Action(nameof(Create), "Booking");
                return View(await BuildFormModelAsync(model));
            }

            // Use new availability orchestration endpoint
            var availabilityResponse = await _service.CreateWithAvailabilityFlowAsync(
                new BookingAvailabilityRequestDto(
                    model.CustomerId,
                    model.RoomId,
                    model.CheckIn,
                    model.CheckOut));

            // Null check for response
            if (availabilityResponse == null)
            {
                var nullError = "No response from booking service. Please try again.";
                if (isAjax) return Json(new { result_type = "VALIDATION_ERROR", success = false, message = nullError });
                ModelState.AddModelError(string.Empty, nullError);
                return View(await BuildFormModelAsync(model));
            }

            // Null or empty check for result_type
            if (string.IsNullOrEmpty(availabilityResponse.result_type))
            {
                var emptyTypeError = "Invalid response from booking service. Please try again.";
                if (isAjax) return Json(new { result_type = "VALIDATION_ERROR", success = false, message = emptyTypeError });
                ModelState.AddModelError(string.Empty, emptyTypeError);
                return View(await BuildFormModelAsync(model));
            }

            // Branch based on result type
            return availabilityResponse.result_type switch
            {
                "BOOKING_CREATED" => HandleBookingCreatedResponse(availabilityResponse, isAjax, model),
                "ALTERNATIVE_ROOM_SUGGESTED" => HandleAlternativeRoomResponse(availabilityResponse, isAjax, model),
                "QUEUED_AND_PENDING_BOOKING_CREATED" => HandleQueuedAndPendingResponse(availabilityResponse, isAjax, model),
                "VALIDATION_ERROR" => HandleValidationErrorResponse(availabilityResponse, isAjax, model),
                _ => StatusCode(500, new { success = false, message = $"Unknown response type from API: {availabilityResponse.result_type}" })
            };
        }
        catch (BookingApiException ex)
        {
            if (isAjax) return Json(new { result_type = "VALIDATION_ERROR", success = false, message = ex.Message });
            ModelState.AddModelError(string.Empty, ex.Message);
            model.PageTitle = "Create Booking";
            model.IntroText = "Open a reservation with guest, room, and stay dates while the API calculates reserve number, status, and pricing.";
            model.SubmitLabel = "Create Booking";
            model.HeroEyebrow = "Reservation Intake";
            model.LockCustomerSelection = false;
            model.AddCustomerReturnUrl = Url.Action(nameof(Create), "Booking");
            return View(await BuildFormModelAsync(model));
        }
        catch (HttpRequestException ex)
        {
            if (isAjax) return Json(new { result_type = "VALIDATION_ERROR", success = false, message = $"API Error: {ex.Message}" });
            ModelState.AddModelError(string.Empty, $"API Error: {ex.Message}");
            model.PageTitle = "Create Booking";
            model.IntroText = "Open a reservation with guest, room, and stay dates while the API calculates reserve number, status, and pricing.";
            model.SubmitLabel = "Create Booking";
            model.HeroEyebrow = "Reservation Intake";
            model.LockCustomerSelection = false;
            model.AddCustomerReturnUrl = Url.Action(nameof(Create), "Booking");
            return View(await BuildFormModelAsync(model));
        }
        catch (Exception ex)
        {
            if (isAjax) return Json(new { result_type = "VALIDATION_ERROR", success = false, message = $"Booking creation failed: {ex.Message}" });
            ModelState.AddModelError(string.Empty, $"Booking creation failed: {ex.Message}");
            model.PageTitle = "Create Booking";
            model.IntroText = "Open a reservation with guest, room, and stay dates while the API calculates reserve number, status, and pricing.";
            model.SubmitLabel = "Create Booking";
            model.HeroEyebrow = "Reservation Intake";
            model.LockCustomerSelection = false;
            model.AddCustomerReturnUrl = Url.Action(nameof(Create), "Booking");
            return View(await BuildFormModelAsync(model));
        }
    }

    private IActionResult HandleBookingCreatedResponse(BookingAvailabilityResponseDto response, bool isAjax, BookingFormViewModel model)
    {
        if (isAjax)
        {
            return Json(new { result_type = "BOOKING_CREATED", success = true, message = response.message, redirectUrl = Url.Action(nameof(List)) });
        }

        TempData["SuccessMessage"] = response.message;
        return RedirectToAction(nameof(List));
    }

    private IActionResult HandleAlternativeRoomResponse(BookingAvailabilityResponseDto response, bool isAjax, BookingFormViewModel model)
    {
        if (isAjax)
        {
            return Json(new 
            { 
                result_type = "ALTERNATIVE_ROOM_SUGGESTED",
                success = false, 
                alternativeAvailable = true, 
                message = response.message,
                alternative_room_suggested = response.alternative_room_suggested 
            });
        }

        // For non-AJAX, show form with prompt
        model.ShowNoAvailabilityPrompt = true;
        model.NoAvailabilityPromptText = $"{response.message} ¿Deseas aceptar la alternativa?";
        model.AlternativeRoomData = JsonSerializer.Serialize(response.alternative_room_suggested);
        TempData["AlternativeRoomMessage"] = response.message;
        
        return View(model);
    }

    private IActionResult HandleQueuedAndPendingResponse(BookingAvailabilityResponseDto response, bool isAjax, BookingFormViewModel model)
    {
        if (isAjax)
        {
            return Json(new 
            { result_type = "QUEUED_AND_PENDING_BOOKING_CREATED",
                success = true, 
                queued = true, 
                message = response.message, 
                queued_and_pending = response.queued_and_pending,
                redirectUrl = Url.Action(nameof(List)) 
            });
        }

        TempData["SuccessMessage"] = response.message;
        TempData["QueuedMessage"] = "Tu solicitud ha sido registrada en la lista de espera. Te contactaremos cuando una habitación esté disponible.";
        return RedirectToAction(nameof(List));
    }

    private IActionResult HandleValidationErrorResponse(BookingAvailabilityResponseDto response, bool isAjax, BookingFormViewModel model)
    {
        if (isAjax)
        {
            return Json(new { result_type = "VALIDATION_ERROR", success = false, message = response.message });
        }

        ModelState.AddModelError(string.Empty, response.message);
        model.PageTitle = "Create Booking";
        model.IntroText = "Open a reservation with guest, room, and stay dates while the API calculates reserve number, status, and pricing.";
        model.SubmitLabel = "Create Booking";
        model.HeroEyebrow = "Reservation Intake";
        model.LockCustomerSelection = false;
        model.AddCustomerReturnUrl = Url.Action(nameof(Create), "Booking");
        
        return View(model);
    }

    /// <summary>
    /// Confirms acceptance of an alternative room suggestion.
    /// </summary>
    [HttpPost("Booking/ConfirmAlternative")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmAlternativeRoom(uint customerId, uint alternativeRoomId, DateTime checkIn, DateTime checkOut)
    {
        bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        try
        {
            var confirmRequest = new BookingAvailabilityRequestDto(customerId, alternativeRoomId, checkIn, checkOut);
            var response = await _service.CreateWithAvailabilityFlowAsync(confirmRequest);

            if (response.result_type == "BOOKING_CREATED")
            {
                if (isAjax)
                    return Json(new { success = true, message = response.message, redirectUrl = Url.Action(nameof(List)) });

                TempData["SuccessMessage"] = response.message;
                return RedirectToAction(nameof(List));
            }

            // Alternative no longer available
            if (isAjax)
                return Json(new { success = false, message = response.message });

            TempData["ErrorMessage"] = response.message;
            return RedirectToAction(nameof(Create), new { customerId });
        }
        catch (Exception ex)
        {
            if (isAjax)
                return Json(new { success = false, message = ex.Message });

            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Create), new { customerId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCustomerInline([FromForm] InlineCustomerCreateViewModel customer)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .SelectMany(entry => entry.Value!.Errors.Select(error => error.ErrorMessage))
                .ToList();

            return BadRequest(new
            {
                success = false,
                message = errors.Count == 0 ? "Invalid customer data." : string.Join(" ", errors)
            });
        }

        try
        {
            var newId = await _customerService.CreateAsync(new SaveCustomerDto(
                customer.UserId,
                customer.DocumentNumber,
                customer.FirstName,
                customer.LastName,
                customer.Phone,
                customer.Address,
                customer.City,
                customer.Country));

            return Ok(new
            {
                success = true,
                customerId = newId,
                customerLabel = $"{customer.FirstName} {customer.LastName} - {customer.DocumentNumber}",
                message = "Customer created and assigned to booking form."
            });
        }
        catch (HttpRequestException)
        {
            return StatusCode(503, new
            {
                success = false,
                message = "Customer service is unavailable. Start HotelCarga.ApiModel and try again."
            });
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
                RoomId = booking.room_id ?? 0,
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
                model.AddCustomerReturnUrl = Url.Action(nameof(Create), "Booking");
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
            model.AddCustomerReturnUrl = Url.Action(nameof(Create), "Booking");
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
            return RedirectToAction(nameof(List));
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

    private async Task<uint> ResolveRoomSelectionAsync(uint? roomId, uint? roomNumber, string? roomType)
    {
        if (roomId.HasValue && roomId.Value > 0)
        {
            return roomId.Value;
        }

        if (!roomNumber.HasValue)
        {
            return 0;
        }

        var rooms = await _roomService.GetAllAsync();
        var matchingRooms = rooms.Where(room => room.room_number == roomNumber.Value);

        if (!string.IsNullOrWhiteSpace(roomType))
        {
            matchingRooms = matchingRooms.Where(room =>
                string.Equals(room.category_name, roomType, StringComparison.OrdinalIgnoreCase));
        }

        return matchingRooms.Select(room => room.id).FirstOrDefault();
    }

    private IActionResult RedirectToIndexWithApiError()
    {
        TempData["ErrorMessage"] = ApiUnavailableMessage;
        return RedirectToAction(nameof(List));
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

    private static string BuildRoomTypeDescription(string categoryName)
    {
        if (categoryName.Contains("suite", StringComparison.OrdinalIgnoreCase))
        {
            return "Ideal para estadias premium con mayor espacio, sala de descanso y comodidades ejecutivas.";
        }

        if (categoryName.Contains("deluxe", StringComparison.OrdinalIgnoreCase))
        {
            return "Disenada para comodidad superior con acabados modernos y ambiente relajado.";
        }

        if (categoryName.Contains("family", StringComparison.OrdinalIgnoreCase) || categoryName.Contains("familiar", StringComparison.OrdinalIgnoreCase))
        {
            return "Pensada para grupos o familias, con distribucion amplia y confort para estancias largas.";
        }

        return "Habitacion funcional y confortable, adecuada para viajes de negocio o descanso.";
    }

    private static bool IsNoAvailabilityMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        return message.Contains("no disponible", StringComparison.OrdinalIgnoreCase)
            || message.Contains("no se encuentra disponible", StringComparison.OrdinalIgnoreCase)
            || message.Contains("lista de espera", StringComparison.OrdinalIgnoreCase)
            || message.Contains("cola", StringComparison.OrdinalIgnoreCase)
            || message.Contains("agenda", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsScheduleConflictMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        return message.Contains("solap", StringComparison.OrdinalIgnoreCase)
            || message.Contains("overlap", StringComparison.OrdinalIgnoreCase)
            || IsNoAvailabilityMessage(message);
    }

    private static bool IsQueueAddedMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        return message.Contains("agregado a la lista de espera", StringComparison.OrdinalIgnoreCase)
            || message.Contains("added to the waiting queue", StringComparison.OrdinalIgnoreCase);
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
        var groups = new Dictionary<string, SelectListGroup>(StringComparer.OrdinalIgnoreCase);

        return rooms
            .Where(room => room.status_id != 3 || room.id == currentRoomId)
            .OrderBy(room => room.category_name ?? string.Empty)
            .ThenBy(room => room.nightly_rate)
            .ThenBy(room => room.room_number)
            .Select(room =>
            {
                var categoryName = room.category_name ?? "Room";
                if (!groups.TryGetValue(categoryName, out var group))
                {
                    group = new SelectListGroup { Name = categoryName };
                    groups[categoryName] = group;
                }

                return new SelectListItem(
                    $"Room {room.room_number} | {room.nightly_rate:C} por noche",
                    room.id.ToString(),
                    room.id == selectedId)
                {
                    Group = group
                };
            })
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
            IsQueueRequest = false,
            ReserveNumber = booking.reserve_number ?? "Pending",
            StatusId = booking.status_id,
            StatusName = booking.status_name,
            CustomerId = booking.customer_id,
            CustomerName = booking.customer_name,
            RoomId = booking.room_id ?? 0,
            RoomNumber = booking.room_number,
            CheckIn = booking.check_in,
            CheckOut = booking.check_out,
            NightlyRate = booking.nightly_rate,
            TotalPrice = booking.total_price
        };
    }

    private static IEnumerable<BookingListItemViewModel> MapQueueEntries(
        IEnumerable<WaitingQueueApiDto> queueEntries,
        IEnumerable<BookingSummaryDto> bookings,
        IEnumerable<RoomDto> rooms)
    {
        var roomCategoriesById = rooms.ToDictionary(room => room.id, room => room.category_id);

        return queueEntries
            .Where(entry => !bookings.Any(booking =>
                booking.status_id == 1
                && booking.customer_id == entry.customer_id
                && booking.check_in.Date == entry.requested_check_in.Date
                && booking.room_id.HasValue
                && roomCategoriesById.TryGetValue(booking.room_id.Value, out var bookingCategoryId)
                && bookingCategoryId == entry.room_category_id))
            .Select(entry => new BookingListItemViewModel
        {
            Id = entry.id,
            WaitingQueueId = entry.id,
            IsQueueRequest = true,
            ReserveNumber = string.IsNullOrWhiteSpace(entry.request_number) ? FormatQueueRequestNumber(entry.id) : entry.request_number.Trim(),
            StatusId = 1,
            StatusName = "PENDING",
            CustomerId = entry.customer_id,
            CustomerName = string.IsNullOrWhiteSpace(entry.customer_name) ? "Unknown" : entry.customer_name,
            RoomId = 0,
            RoomNumber = 0,
            CheckIn = entry.requested_check_in,
            CheckOut = entry.check_out ?? entry.requested_check_in,
            NightlyRate = 0,
            TotalPrice = 0
        });
    }

    private async Task<List<WaitingQueueApiDto>> FetchPendingQueueEntriesAsync()
    {
        var response = await _httpClient.GetAsync("WaitingQueue/GetAll");
        response.EnsureSuccessStatusCode();

        var allEntries = await response.Content.ReadFromJsonAsync<List<WaitingQueueApiDto>>(JsonOptions) ?? [];
        return allEntries
            .Where(entry => string.Equals(entry.status_name?.Trim(), "PENDING", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private static string FormatQueueRequestNumber(uint id)
    {
        var token = unchecked(id * 2654435761u);
        return $"RQ-{token:X8}";
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

        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            parts.Add($"search matches '{filters.SearchTerm}'");
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