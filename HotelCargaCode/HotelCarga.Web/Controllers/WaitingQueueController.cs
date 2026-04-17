using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using HotelCarga.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HotelCarga.Web.Controllers;

public class WaitingQueueController : Controller
{
    private readonly HttpClient _httpClient;

    public WaitingQueueController(IHttpClientFactory clientFactory, IOptions<ApiSettings> settings)
    {
        _httpClient = clientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(settings.Value.BaseUrl);
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
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

    [HttpPost("Api/Create")]
    public async Task<IActionResult> Create([FromBody] JsonElement payload, bool useJson = false)
    {
        var response = await _httpClient.PostAsJsonAsync($"WaitingQueue/Create?useJson={ToApiBoolean(useJson)}", payload);
        return await BuildProxyResultAsync(response);
    }

    [HttpPut("Api/Update")]
    public async Task<IActionResult> Update([FromBody] JsonElement payload, bool useJson = false)
    {
        var response = await _httpClient.PutAsJsonAsync($"WaitingQueue/Update?useJson={ToApiBoolean(useJson)}", payload);
        return await BuildProxyResultAsync(response);
    }

    [HttpDelete("Api/Delete")]
    public async Task<IActionResult> Delete(uint id, bool useJson = false)
    {
        var response = await _httpClient.DeleteAsync($"WaitingQueue/Delete?id={id}&useJson={ToApiBoolean(useJson)}");
        return await BuildProxyResultAsync(response);
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

    private async Task<IActionResult> ForwardGetAsync(string relativeUrl)
    {
        var response = await _httpClient.GetAsync(relativeUrl);
        return await BuildProxyResultAsync(response);
    }

    private static string ToApiBoolean(bool value)
    {
        return value ? "true" : "false";
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
