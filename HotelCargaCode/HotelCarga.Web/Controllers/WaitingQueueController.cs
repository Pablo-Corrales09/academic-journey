using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HotelCarga.Models.WaitingQueue;
using HotelCarga.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HotelCarga.Web.Controllers;

[Route("[controller]")]
public class WaitingQueueController : Controller
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    public WaitingQueueController(IHttpClientFactory clientFactory, IOptions<ApiSettings> settings)
    {
        _httpClient = clientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(settings.Value.BaseUrl);
    }

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] WaitingQueueFiltersViewModel filters, bool useJson = false)
    {
        filters.StatusName = NormalizeStatus(filters.StatusName);
        if (string.IsNullOrWhiteSpace(filters.StatusName))
        {
            filters.StatusName = "PENDING";
        }

        if (filters.FromDate.HasValue && filters.ToDate.HasValue && filters.FromDate.Value.Date > filters.ToDate.Value.Date)
        {
            ModelState.AddModelError(string.Empty, "From date cannot be later than To date.");
        }

        var model = new WaitingQueueIndexViewModel
        {
            Filters = filters,
            Items = []
        };

        try
        {
            var entries = await FetchQueueAsync(useJson);
            var filteredItems = ApplyFilters(entries, filters)
                .Select(MapToListItem)
                .OrderBy(item => item.RequestedCheckIn)
                .ThenBy(item => item.CreatedAt ?? DateTime.MaxValue)
                .ToList();

            model = new WaitingQueueIndexViewModel
            {
                Filters = filters,
                Items = filteredItems
            };
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] = "Waiting Queue API is unavailable. Start HotelCarga.ApiModel and verify ApiSettings:BaseUrl.";
        }

        return View(model);
    }

    [HttpGet("Api/GetById")]
    public async Task<IActionResult> GetById(uint id, bool useJson = false)
    {
        return await ForwardGetAsync($"WaitingQueue/GetById?id={id}&useJson={ToApiBoolean(useJson)}");
    }

    [HttpGet("Api/GetAll")]
    public async Task<IActionResult> GetAll(bool useJson = false)
    {
        return await ForwardGetAsync($"WaitingQueue/GetAll?useJson={ToApiBoolean(useJson)}");
    }

    [HttpPut("Api/Update")]
    public async Task<IActionResult> Update([FromBody] JsonElement payload, bool useJson = false)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"WaitingQueue/Update?useJson={ToApiBoolean(useJson)}", payload);
            return await BuildProxyResultAsync(response);
        }
        catch (HttpRequestException ex)
        {
            return BuildApiUnavailableResult("WaitingQueue/Update", ex);
        }
    }

    [HttpDelete("Api/Delete")]
    public async Task<IActionResult> Delete(uint id, bool useJson = false)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"WaitingQueue/Delete?id={id}&useJson={ToApiBoolean(useJson)}");
            return await BuildProxyResultAsync(response);
        }
        catch (HttpRequestException ex)
        {
            return BuildApiUnavailableResult("WaitingQueue/Delete", ex);
        }
    }

    [HttpGet("Api/GetCategoriesByCustomerId")]
    public async Task<IActionResult> GetCategoriesByCustomerId(uint customerId, bool useJson = false)
    {
        return await ForwardGetAsync($"WaitingQueue/GetCategoriesByCustomerId?customerId={customerId}&useJson={ToApiBoolean(useJson)}");
    }

    [HttpGet("Api/GetWithRelatedEntitiesByCustomerId")]
    public async Task<IActionResult> GetWithRelatedEntitiesByCustomerId(uint customerId, bool useJson = false)
    {
        return await ForwardGetAsync($"WaitingQueue/GetWithRelatedEntitiesByCustomerId?customerId={customerId}&useJson={ToApiBoolean(useJson)}");
    }

    [HttpGet("Api/GetCategoriesByQueueStatusName")]
    public async Task<IActionResult> GetCategoriesByQueueStatusName(string statusName, bool useJson = false)
    {
        var escaped = Uri.EscapeDataString(statusName);
        return await ForwardGetAsync($"WaitingQueue/GetCategoriesByQueueStatusName?statusName={escaped}&useJson={ToApiBoolean(useJson)}");
    }

    [HttpGet("Api/GetWithCustomerByQueueStatusName")]
    public async Task<IActionResult> GetWithCustomerByQueueStatusName(string statusName, bool useJson = false)
    {
        var escaped = Uri.EscapeDataString(statusName);
        return await ForwardGetAsync($"WaitingQueue/GetWithCustomerByQueueStatusName?statusName={escaped}&useJson={ToApiBoolean(useJson)}");
    }

    [HttpGet("Api/GetAllQueues")]
    public async Task<IActionResult> GetAllQueues(bool useJson = false)
    {
        return await ForwardGetAsync($"WaitingQueue/GetAllQueues?useJson={ToApiBoolean(useJson)}");
    }

    [HttpGet("Api/GetFIFOQueueByRoomCategoryAndDateRange")]
    public async Task<IActionResult> GetFIFOQueueByRoomCategoryAndDateRange(byte roomCategoryId, DateTime checkInDate, DateTime checkOutDate, bool useJson = false)
    {
        var checkIn = Uri.EscapeDataString(checkInDate.ToString("O", CultureInfo.InvariantCulture));
        var checkOut = Uri.EscapeDataString(checkOutDate.ToString("O", CultureInfo.InvariantCulture));
        return await ForwardGetAsync($"WaitingQueue/GetFIFOQueueByRoomCategoryAndDateRange?roomCategoryId={roomCategoryId}&checkInDate={checkIn}&checkOutDate={checkOut}&useJson={ToApiBoolean(useJson)}");
    }

    [HttpGet("Api/GetFIFOQueueByRoomCategory")]
    public async Task<IActionResult> GetFIFOQueueByRoomCategory(byte roomCategoryId, bool useJson = false)
    {
        return await ForwardGetAsync($"WaitingQueue/GetFIFOQueueByRoomCategory?roomCategoryId={roomCategoryId}&useJson={ToApiBoolean(useJson)}");
    }

    [HttpGet("Api/GetNotifiedQueueFIFO")]
    public async Task<IActionResult> GetNotifiedQueueFIFO(bool useJson = false)
    {
        return await ForwardGetAsync($"WaitingQueue/GetNotifiedQueueFIFO?useJson={ToApiBoolean(useJson)}");
    }

    [HttpGet("Api/CountPendingByRoomCategory")]
    public async Task<IActionResult> CountPendingByRoomCategory(byte roomCategoryId, bool useJson = false)
    {
        return await ForwardGetAsync($"WaitingQueue/CountPendingByRoomCategory?roomCategoryId={roomCategoryId}&useJson={ToApiBoolean(useJson)}");
    }

    private async Task<List<WaitingQueueApiDto>> FetchQueueAsync(bool useJson)
    {
        var response = await _httpClient.GetAsync($"WaitingQueue/GetAll?useJson={ToApiBoolean(useJson)}");
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return [];
            }

            var rawBody = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"GET WaitingQueue/GetAll failed ({(int)response.StatusCode}): {rawBody}");
        }

        return await response.Content.ReadFromJsonAsync<List<WaitingQueueApiDto>>(JsonOptions) ?? [];
    }

    private static IEnumerable<WaitingQueueApiDto> ApplyFilters(IEnumerable<WaitingQueueApiDto> items, WaitingQueueFiltersViewModel filters)
    {
        var query = items;

        var status = NormalizeStatus(filters.StatusName);
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(item => NormalizeStatus(item.status_name) == status);
        }

        if (filters.FromDate.HasValue)
        {
            var fromDate = filters.FromDate.Value.Date;
            query = query.Where(item => item.requested_check_in.Date >= fromDate);
        }

        if (filters.ToDate.HasValue)
        {
            var toDate = filters.ToDate.Value.Date;
            query = query.Where(item => item.requested_check_in.Date <= toDate);
        }

        return query;
    }

    private static WaitingQueueListItemViewModel MapToListItem(WaitingQueueApiDto item)
    {
        return new WaitingQueueListItemViewModel
        {
            Id = item.id,
            RequestNumber = string.IsNullOrWhiteSpace(item.request_number) ? FormatRequestNumber(item.id) : item.request_number.Trim(),
            CustomerId = item.customer_id,
            CustomerName = string.IsNullOrWhiteSpace(item.customer_name) ? "Unknown" : item.customer_name,
            RoomCategoryId = item.room_category_id,
            RoomCategoryName = string.IsNullOrWhiteSpace(item.room_category_name) ? "Unknown" : item.room_category_name,
            StatusId = item.status_id,
            StatusName = NormalizeStatus(item.status_name),
            RequestedCheckIn = item.requested_check_in,
            CheckOut = item.check_out,
            CreatedAt = item.created_at,
            UpdatedAt = item.updated_at
        };
    }

    private async Task<IActionResult> ForwardGetAsync(string relativeUrl)
    {
        try
        {
            var response = await _httpClient.GetAsync(relativeUrl);
            return await BuildProxyResultAsync(response);
        }
        catch (HttpRequestException ex)
        {
            return BuildApiUnavailableResult(relativeUrl, ex);
        }
    }

    private IActionResult BuildApiUnavailableResult(string endpoint, HttpRequestException ex)
    {
        return StatusCode(503, new
        {
            success = false,
            message = "Waiting Queue API is unavailable. Start HotelCarga.ApiModel and verify ApiSettings:BaseUrl.",
            endpoint,
            detail = ex.Message
        });
    }

    private static string NormalizeStatus(string? statusName)
    {
        return (statusName ?? string.Empty).Trim().ToUpperInvariant();
    }

    private static string ToApiBoolean(bool value)
    {
        return value ? "true" : "false";
    }

    private static string FormatRequestNumber(uint id)
    {
        var token = unchecked(id * 2654435761u);
        return $"RQ-{token:X8}";
    }

    private static async Task<ContentResult> BuildProxyResultAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";

        return new ContentResult
        {
            StatusCode = (int)response.StatusCode,
            Content = content,
            ContentType = contentType
        };
    }
}
