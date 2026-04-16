using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace HotelCarga.Web.Services;

public record BookingSummaryDto(
    uint id,
    string? reserve_number,
    byte status_id,
    string status_name,
    uint customer_id,
    string customer_name,
    uint room_id,
    uint room_number,
    DateTime check_in,
    DateTime check_out,
    decimal nightly_rate,
    decimal total_price);

public record BookingStatusDto(byte id, string status_name, string? description, DateTime? created_at);

public record SaveBookingDto(uint customer_id, uint room_id, DateTime check_in, DateTime check_out, byte? status_id = null);

public record BookingMutationEnvelope(bool success, string? message, BookingSummaryDto? data);

public record BookingDeleteEnvelope(bool success, string? message);

public sealed class BookingApiException : Exception
{
    public BookingApiException(string message) : base(message)
    {
    }
}

public interface IBookingApiService
{
    Task<List<BookingSummaryDto>> GetAllAsync();
    Task<BookingSummaryDto?> GetByIdAsync(uint id);
    Task<uint> CreateAsync(SaveBookingDto dto, bool addToQueueIfUnavailable = false);
    Task UpdateAsync(uint id, SaveBookingDto dto);
    Task DeleteAsync(uint id);
    Task<string?> GetReserveNumberByIdAsync(uint id);
    Task<uint?> GetRoomIdByReserveNumberAsync(string reserveNumber);
    Task<List<uint>> GetBookingHistoryIdsByReserveNumberAsync(string reserveNumber);
    Task<List<string>> GetOverlappingReserveNumbersAsync(DateTime startDate, DateTime endDate);
    Task<List<string>> GetReserveNumbersByCheckInDateAsync(DateTime checkInDate);
    Task<decimal?> GetTotalPriceByReserveNumberAsync(string reserveNumber);
    Task<List<string>> GetReserveNumbersByStatusIdAsync(byte statusId);
    Task<List<string>> GetReserveNumbersByCustomerIdAsync(uint customerId);
    Task<List<BookingStatusDto>> GetStatusesAsync();
}

public class BookingApiService : IBookingApiService
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public BookingApiService(IHttpClientFactory clientFactory, IOptions<ApiSettings> settings)
    {
        _httpClient = clientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(settings.Value.BaseUrl);
    }

    public async Task<List<BookingSummaryDto>> GetAllAsync()
    {
        return await GetRequiredAsync<List<BookingSummaryDto>>("Booking/GetAll") ?? [];
    }

    public async Task<BookingSummaryDto?> GetByIdAsync(uint id)
    {
        return await GetOptionalAsync<BookingSummaryDto>($"Booking/GetById?id={id}");
    }

    public async Task<uint> CreateAsync(SaveBookingDto dto, bool addToQueueIfUnavailable = false)
    {
        var endpoint = $"Booking/Create?addToQueueIfUnavailable={addToQueueIfUnavailable.ToString().ToLowerInvariant()}";
        var response = await _httpClient.PostAsJsonAsync(endpoint, dto);
        if (!response.IsSuccessStatusCode)
        {
            throw await BuildApiExceptionAsync(response, "Booking creation failed.");
        }

        var payload = await response.Content.ReadFromJsonAsync<BookingMutationEnvelope>(JsonOptions)
            ?? throw new InvalidOperationException("Booking create response was empty.");

        return payload.data?.id ?? throw new InvalidOperationException("Booking create response did not include booking data.");
    }

    public async Task UpdateAsync(uint id, SaveBookingDto dto)
    {
        var request = new
        {
            id,
            dto.customer_id,
            dto.room_id,
            dto.check_in,
            dto.check_out,
            status_id = dto.status_id ?? 0
        };

        var response = await _httpClient.PutAsJsonAsync("Booking/Update", request);
        if (!response.IsSuccessStatusCode)
        {
            throw await BuildApiExceptionAsync(response, "Booking update failed.");
        }

        _ = await response.Content.ReadFromJsonAsync<BookingMutationEnvelope>(JsonOptions);
    }

    public async Task DeleteAsync(uint id)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, "Booking/Delete")
        {
            Content = JsonContent.Create(new { id })
        };

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw await BuildApiExceptionAsync(response, "Booking cancellation failed.");
        }

        _ = await response.Content.ReadFromJsonAsync<BookingDeleteEnvelope>(JsonOptions);
    }

    public async Task<string?> GetReserveNumberByIdAsync(uint id)
    {
        return await GetOptionalAsync<string>($"Booking/GetReserveNumberById?id={id}");
    }

    public async Task<uint?> GetRoomIdByReserveNumberAsync(string reserveNumber)
    {
        return await GetOptionalAsync<uint?>($"Booking/GetRoomIdByReserveNumber?reserveNumber={Uri.EscapeDataString(reserveNumber)}");
    }

    public async Task<List<uint>> GetBookingHistoryIdsByReserveNumberAsync(string reserveNumber)
    {
        return await GetRequiredAsync<List<uint>>($"Booking/GetBookingHistoryIdsByReserveNumber?reserveNumber={Uri.EscapeDataString(reserveNumber)}") ?? [];
    }

    public async Task<List<string>> GetOverlappingReserveNumbersAsync(DateTime startDate, DateTime endDate)
    {
        var start = Uri.EscapeDataString(startDate.ToString("O"));
        var end = Uri.EscapeDataString(endDate.ToString("O"));
        return await GetRequiredAsync<List<string>>($"Booking/GetOverlappingReserveNumbers?startDate={start}&endDate={end}") ?? [];
    }

    public async Task<List<string>> GetReserveNumbersByCheckInDateAsync(DateTime checkInDate)
    {
        var date = Uri.EscapeDataString(checkInDate.ToString("O"));
        return await GetRequiredAsync<List<string>>($"Booking/GetReserveNumbersByCheckInDate?checkInDate={date}") ?? [];
    }

    public async Task<decimal?> GetTotalPriceByReserveNumberAsync(string reserveNumber)
    {
        return await GetOptionalAsync<decimal?>($"Booking/GetTotalPriceByReserveNumber?reserveNumber={Uri.EscapeDataString(reserveNumber)}");
    }

    public async Task<List<string>> GetReserveNumbersByStatusIdAsync(byte statusId)
    {
        return await GetRequiredAsync<List<string>>($"Booking/GetReserveNumbersByStatusId?statusId={statusId}") ?? [];
    }

    public async Task<List<string>> GetReserveNumbersByCustomerIdAsync(uint customerId)
    {
        return await GetRequiredAsync<List<string>>($"Booking/GetReserveNumbersByCustomerId?customerId={customerId}") ?? [];
    }

    public async Task<List<BookingStatusDto>> GetStatusesAsync()
    {
        return await GetRequiredAsync<List<BookingStatusDto>>("BookingStatus/GetAll") ?? [];
    }

    private async Task<T?> GetOptionalAsync<T>(string url)
    {
        var response = await _httpClient.GetAsync(url);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    private async Task<T> GetRequiredAsync<T>(string url)
    {
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions)
            ?? throw new InvalidOperationException($"A required response for '{url}' was empty.");
    }

    private static async Task<BookingApiException> BuildApiExceptionAsync(HttpResponseMessage response, string fallbackMessage)
    {
        try
        {
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if (document.RootElement.TryGetProperty("message", out var messageElement))
            {
                var message = messageElement.GetString();
                if (!string.IsNullOrWhiteSpace(message))
                {
                    return new BookingApiException(message);
                }
            }
        }
        catch (JsonException)
        {
        }

        return new BookingApiException(fallbackMessage);
    }
}