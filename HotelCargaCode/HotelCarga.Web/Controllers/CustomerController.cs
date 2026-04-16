using HotelCarga.Models.Customers;
using HotelCarga.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Json;

namespace HotelCarga.Web.Controllers;

public class CustomerController : Controller
{
    private const string ApiUnavailableMessage = "Customer service is unavailable. Start HotelCarga.ApiModel and try again.";

    private readonly ICustomerApiService _service;

    public CustomerController(ICustomerApiService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index(CustomerFiltersViewModel filters)
    {
        List<CustomerSummaryDto> all;
        try
        {
            all = await _service.GetAllAsync();
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] = ApiUnavailableMessage;
            return View(BuildUnavailableIndexModel(filters));
        }

        var filtered = all.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filters.Name))
        {
            filtered = filtered.Where(customer =>
                customer.first_name.Contains(filters.Name, StringComparison.OrdinalIgnoreCase) ||
                customer.last_name.Contains(filters.Name, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filters.DocumentNumber))
        {
            filtered = filtered.Where(customer => customer.document_number.Contains(filters.DocumentNumber, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filters.Phone))
        {
            filtered = filtered.Where(customer => customer.phone.Contains(filters.Phone, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filters.Country))
        {
            filtered = filtered.Where(customer => customer.country.Contains(filters.Country, StringComparison.OrdinalIgnoreCase));
        }

        if (filters.HasLinkedUser.HasValue)
        {
            filtered = filtered.Where(customer => !string.IsNullOrWhiteSpace(customer.username) == filters.HasLinkedUser.Value);
        }

        var list = filtered
            .OrderBy(customer => customer.last_name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(customer => customer.first_name, StringComparer.OrdinalIgnoreCase)
            .Select(customer => MapSummary(customer))
            .ToList();

        return View(new CustomerIndexViewModel
        {
            Filters = filters,
            Customers = list,
            ActiveFilterSummary = BuildFilterSummary(filters),
            Stats = new CustomerIndexStatsViewModel
            {
                TotalCustomers = all.Count,
                MatchingCustomers = list.Count,
                LinkedUsers = all.Count(customer => !string.IsNullOrWhiteSpace(customer.username)),
                ActiveLinkedUsers = all.Count(customer =>
                    !string.IsNullOrWhiteSpace(customer.username) &&
                    string.Equals(customer.status, "Active", StringComparison.OrdinalIgnoreCase))
            }
        });
    }

    public async Task<IActionResult> Details(uint id)
    {
        try
        {
            var summaryTask = _service.GetByIdAsync(id);
            var bookingsTask = _service.GetBookingIdsByCustomerIdAsync(id);
            var historiesTask = _service.GetBookingHistoryIdsByCustomerIdAsync(id);
            var queuesTask = _service.GetWaitingQueueIdsByCustomerIdAsync(id);

            await Task.WhenAll(summaryTask, bookingsTask, historiesTask, queuesTask);

            var summary = summaryTask.Result;
            if (summary is null)
            {
                return NotFound();
            }

            var customerByUser = string.IsNullOrWhiteSpace(summary.username)
                ? null
                : await _service.SearchByNameAsync(summary.first_name);

            var matchedEntity = customerByUser?.FirstOrDefault(c =>
                string.Equals(c.document_number, summary.document_number, StringComparison.OrdinalIgnoreCase));

            return View(new CustomerDetailsViewModel
            {
                Customer = MapSummary(summary, matchedEntity?.user_id),
                BookingIds = bookingsTask.Result,
                BookingHistoryIds = historiesTask.Result,
                WaitingQueueIds = queuesTask.Result,
                CreatedAt = matchedEntity?.created_at,
                UpdatedAt = matchedEntity?.updated_at
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

    public IActionResult SearchByName(string searchString)
    {
        return RedirectToAction(nameof(LookupSearchByName), new { searchString });
    }

    public async Task<IActionResult> Create(string? returnUrl = null, uint? userId = null)
    {
        try
        {
            _ = await _service.GetAllAsync();
            return View(new CustomerFormViewModel
            {
                UserId = userId ?? 0,
                PageTitle = "Create Customer",
                IntroText = "Register a guest profile with identity, contact details, and user account linkage for complete service history.",
                SubmitLabel = "Create Customer",
                HeroEyebrow = "Guest Profile Setup",
                ReturnUrl = returnUrl
            });
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerFormViewModel model)
    {
        try
        {
            await ValidateDuplicateDocumentAsync(model);
            await ValidateDuplicateUserAsync(model);

            if (!ModelState.IsValid)
            {
                model.PageTitle = "Create Customer";
                model.IntroText = "Register a guest profile with identity, contact details, and user account linkage for complete service history.";
                model.SubmitLabel = "Create Customer";
                model.HeroEyebrow = "Guest Profile Setup";
                return View(model);
            }

            var id = await _service.CreateAsync(ToSaveModel(model));

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl))
            {
                var redirectUrl = QueryHelpers.AddQueryString(model.ReturnUrl, new Dictionary<string, string?>
                {
                    ["customerId"] = id.ToString(),
                    ["customerCreated"] = "1"
                });

                return Redirect(redirectUrl);
            }

            TempData["SuccessMessage"] = $"Customer {model.FirstName} {model.LastName} created successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    public async Task<IActionResult> Update(uint id)
    {
        try
        {
            var summary = await _service.GetByIdAsync(id);
            if (summary is null)
            {
                return NotFound();
            }

            var userLinked = await ResolveUserIdAsync(summary);

            return View(new CustomerFormViewModel
            {
                Id = summary.id,
                UserId = userLinked,
                DocumentNumber = summary.document_number,
                FirstName = summary.first_name,
                LastName = summary.last_name,
                Phone = summary.phone,
                Address = summary.address,
                City = summary.city,
                Country = summary.country,
                PageTitle = "Update Customer",
                IntroText = "Refine the customer profile while preserving API relationship endpoints for bookings and waiting queue flows.",
                SubmitLabel = "Save Changes",
                HeroEyebrow = "Guest Profile Refresh"
            });
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(uint id, CustomerFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        try
        {
            await ValidateDuplicateDocumentAsync(model, id);
            await ValidateDuplicateUserAsync(model, id);

            if (!ModelState.IsValid)
            {
                model.PageTitle = "Update Customer";
                model.IntroText = "Refine the customer profile while preserving API relationship endpoints for bookings and waiting queue flows.";
                model.SubmitLabel = "Save Changes";
                model.HeroEyebrow = "Guest Profile Refresh";
                return View(model);
            }

            await _service.UpdateAsync(id, ToSaveModel(model));
            TempData["SuccessMessage"] = $"Customer {model.FirstName} {model.LastName} updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
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
            var summary = await _service.GetByIdAsync(id);
            if (summary is null)
            {
                return NotFound();
            }

            return View(MapSummary(summary, await ResolveUserIdAsync(summary)));
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
            TempData["SuccessMessage"] = "Customer deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    public async Task<IActionResult> LookupSearchByName(string searchString)
    {
        try
        {
            var results = await _service.SearchByNameAsync(searchString);
            var mapped = results.Select(MapEntity).OrderBy(customer => customer.LastName).ToList();

            return View("Lookup", new CustomerLookupResultViewModel
            {
                Title = "Search By Name",
                Description = "Use the API endpoint that searches first and last names in the customer registry.",
                QueryLabel = "Search",
                QueryValue = searchString,
                ResultLabel = "Matching customers",
                CustomerListResult = mapped,
                NotFoundMessage = mapped.Count == 0 ? "No customers matched this name search." : null
            });
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    public async Task<IActionResult> GetByDocumentNumber(string documentNumber)
    {
        try
        {
            return View("Lookup", await BuildEntityLookupAsync(
                title: "Get By Document Number",
                description: "Resolve a customer profile by document number.",
                queryLabel: "Document",
                queryValue: documentNumber,
                entityTask: _service.GetByDocumentNumberAsync(documentNumber)));
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    public async Task<IActionResult> GetByPhone(string phone)
    {
        try
        {
            return View("Lookup", await BuildEntityLookupAsync(
                title: "Get By Phone",
                description: "Resolve a customer profile by phone number.",
                queryLabel: "Phone",
                queryValue: phone,
                entityTask: _service.GetByPhoneAsync(phone)));
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    public async Task<IActionResult> GetByDocumentNumberOrPhone(string documentNumber, string phone)
    {
        try
        {
            return View("Lookup", await BuildEntityLookupAsync(
                title: "Get By Document Number Or Phone",
                description: "Resolve the first customer that matches either the document number or phone.",
                queryLabel: "Document/Phone",
                queryValue: $"{documentNumber} / {phone}",
                entityTask: _service.GetByDocumentNumberOrPhoneAsync(documentNumber, phone)));
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    public async Task<IActionResult> GetByUserId(uint userId)
    {
        try
        {
            return View("Lookup", await BuildEntityLookupAsync(
                title: "Get By User Id",
                description: "Resolve a customer profile from the linked user id.",
                queryLabel: "User Id",
                queryValue: userId.ToString(),
                entityTask: _service.GetByUserIdAsync(userId)));
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    public async Task<IActionResult> GetBookingIdsByCustomerId(uint customerId)
    {
        return await BuildIdLookupAsync(
            title: "Booking Ids by Customer",
            description: "List booking ids that belong to the customer.",
            queryLabel: "Customer Id",
            queryValue: customerId.ToString(),
            resultLabel: "Booking Ids",
            request: _service.GetBookingIdsByCustomerIdAsync(customerId));
    }

    public async Task<IActionResult> GetBookingHistoryIdsByCustomerId(uint customerId)
    {
        return await BuildIdLookupAsync(
            title: "Booking History Ids by Customer",
            description: "List booking history ids that belong to the customer.",
            queryLabel: "Customer Id",
            queryValue: customerId.ToString(),
            resultLabel: "Booking History Ids",
            request: _service.GetBookingHistoryIdsByCustomerIdAsync(customerId));
    }

    public async Task<IActionResult> GetWaitingQueueIdsByCustomerId(uint customerId)
    {
        return await BuildIdLookupAsync(
            title: "Waiting Queue Ids by Customer",
            description: "List waiting queue ids that belong to the customer.",
            queryLabel: "Customer Id",
            queryValue: customerId.ToString(),
            resultLabel: "Waiting Queue Ids",
            request: _service.GetWaitingQueueIdsByCustomerIdAsync(customerId));
    }

    private async Task<IActionResult> BuildIdLookupAsync(
        string title,
        string description,
        string queryLabel,
        string queryValue,
        string resultLabel,
        Task<List<uint>> request)
    {
        try
        {
            var ids = await request;
            return View("Lookup", new CustomerLookupResultViewModel
            {
                Title = title,
                Description = description,
                QueryLabel = queryLabel,
                QueryValue = queryValue,
                ResultLabel = resultLabel,
                IdListResult = ids,
                NotFoundMessage = ids.Count == 0 ? "No records were returned for this relationship endpoint." : null
            });
        }
        catch (HttpRequestException)
        {
            return RedirectToIndexWithApiError();
        }
    }

    private async Task<CustomerLookupResultViewModel> BuildEntityLookupAsync(
        string title,
        string description,
        string queryLabel,
        string queryValue,
        Task<CustomerEntityDto?> entityTask)
    {
        var entity = await entityTask;
        return new CustomerLookupResultViewModel
        {
            Title = title,
            Description = description,
            QueryLabel = queryLabel,
            QueryValue = queryValue,
            ResultLabel = "Customer payload",
            CustomerResult = entity is null ? null : MapEntity(entity),
            NotFoundMessage = entity is null ? "The API did not return a customer for this lookup." : null
        };
    }

    private async Task ValidateDuplicateDocumentAsync(CustomerFormViewModel model, uint? currentCustomerId = null)
    {
        var all = await _service.GetAllAsync();
        var duplicate = all.Any(customer =>
            string.Equals(customer.document_number, model.DocumentNumber, StringComparison.OrdinalIgnoreCase) &&
            (!currentCustomerId.HasValue || customer.id != currentCustomerId.Value));

        if (duplicate)
        {
            ModelState.AddModelError(nameof(model.DocumentNumber), $"Document '{model.DocumentNumber}' already exists.");
        }
    }

    private async Task ValidateDuplicateUserAsync(CustomerFormViewModel model, uint? currentCustomerId = null)
    {
        var existing = await _service.GetByUserIdAsync(model.UserId);
        if (existing is not null && (!currentCustomerId.HasValue || existing.id != currentCustomerId.Value))
        {
            ModelState.AddModelError(nameof(model.UserId), $"User id '{model.UserId}' is already assigned to another customer.");
        }
    }

    private async Task<uint> ResolveUserIdAsync(CustomerSummaryDto summary)
    {
        var byDocument = await _service.GetByDocumentNumberAsync(summary.document_number);
        return byDocument?.user_id ?? 0;
    }

    private IActionResult RedirectToIndexWithApiError()
    {
        TempData["ErrorMessage"] = ApiUnavailableMessage;
        return RedirectToAction(nameof(Index));
    }

    private static CustomerIndexViewModel BuildUnavailableIndexModel(CustomerFiltersViewModel filters)
    {
        return new CustomerIndexViewModel
        {
            Filters = filters,
            Customers = [],
            ActiveFilterSummary = "Customer API is offline.",
            Stats = new CustomerIndexStatsViewModel()
        };
    }

    private static string BuildFilterSummary(CustomerFiltersViewModel filters)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(filters.Name))
        {
            parts.Add($"name contains '{filters.Name}'");
        }

        if (!string.IsNullOrWhiteSpace(filters.DocumentNumber))
        {
            parts.Add($"document contains '{filters.DocumentNumber}'");
        }

        if (!string.IsNullOrWhiteSpace(filters.Phone))
        {
            parts.Add($"phone contains '{filters.Phone}'");
        }

        if (!string.IsNullOrWhiteSpace(filters.Country))
        {
            parts.Add($"country contains '{filters.Country}'");
        }

        if (filters.HasLinkedUser.HasValue)
        {
            parts.Add(filters.HasLinkedUser.Value ? "linked user required" : "without linked user");
        }

        return parts.Count == 0 ? "Showing the full customer registry." : $"Filtered by {string.Join(", ", parts)}.";
    }

    private static SaveCustomerDto ToSaveModel(CustomerFormViewModel model)
    {
        return new SaveCustomerDto(
            model.UserId,
            model.DocumentNumber,
            model.FirstName,
            model.LastName,
            model.Phone,
            model.Address,
            model.City,
            model.Country);
    }

    private static CustomerListItemViewModel MapSummary(CustomerSummaryDto dto, uint? userId = null)
    {
        return new CustomerListItemViewModel
        {
            Id = dto.id,
            UserId = userId,
            FirstName = dto.first_name,
            LastName = dto.last_name,
            DocumentNumber = dto.document_number,
            Phone = dto.phone,
            Address = dto.address,
            City = dto.city,
            Country = dto.country,
            Username = dto.username ?? "Unassigned",
            Email = dto.email ?? "Unassigned",
            RoleName = dto.role ?? "Unassigned",
            StatusName = dto.status ?? "Unassigned",
            BookingsCount = dto.bookings.ValueKind == JsonValueKind.Array ? dto.bookings.GetArrayLength() : 0,
            WaitingQueuesCount = dto.waiting_queues.ValueKind == JsonValueKind.Array ? dto.waiting_queues.GetArrayLength() : 0
        };
    }

    private static CustomerListItemViewModel MapEntity(CustomerEntityDto dto)
    {
        return new CustomerListItemViewModel
        {
            Id = dto.id,
            UserId = dto.user_id,
            FirstName = dto.first_name,
            LastName = dto.last_name,
            DocumentNumber = dto.document_number,
            Phone = dto.phone,
            Address = dto.address,
            City = dto.city,
            Country = dto.country,
            Username = "Available via GetById",
            Email = "Available via GetById",
            RoleName = "Available via GetById",
            StatusName = "Available via GetById"
        };
    }
}