using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using System.Text.Json;
using System.Net.Http.Json;

namespace HotelCarga.Web.Services;

public class ApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
}

public record RoomDto(uint id, uint room_number, byte? floor_number, decimal nightly_rate, byte status_id, byte category_id, string? status_name, string? category_name);

public record CreateRoomDto(
    uint room_number,
    byte status_id,
    byte category_id,
    decimal nightly_rate,
    byte? floor_number);

public record BookingDto(uint id, string reserveNumber, uint customerId, string customerName, string statusName, DateTime checkIn, DateTime checkOut, decimal totalPrice);

public record BookingHistoryDto(uint id, uint bookingId, uint customerId, string actionType, string statusName, DateTime checkIn, DateTime checkOut, DateTime createdAt, decimal totalPrice);

public record RoomAvailabilityDto(uint id, DateTime startSchedule, DateTime endSchedule, DateTime createdAt);

public interface IRoomApiService
{
    Task<List<RoomDto>> GetAllAsync();
    Task<RoomDto?> GetByIdAsync(uint id);
    Task<RoomDto> CreateAsync(CreateRoomDto dto);
    Task<RoomDto> UpdateAsync(RoomDto dto);
    Task DeleteAsync(uint id);
    Task<RoomDto?> GetByRoomNumberAsync(uint roomNumber);
    Task<List<RoomDto>> GetByNightlyRateRangeAsync(decimal min, decimal max);
    Task<bool> RoomNumberExistsAsync(uint roomNumber, uint? excludeId = null);
    Task<List<BookingDto>> GetBookingsByRoomIdAsync(uint roomId);
    Task<List<BookingHistoryDto>> GetBookingHistoriesByRoomIdAsync(uint roomId);
    Task<List<RoomAvailabilityDto>> GetAvailabilitiesByRoomIdAsync(uint roomId);
}

public class RoomApiService : IRoomApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public RoomApiService(IHttpClientFactory clientFactory, IOptions<ApiSettings> settings)
    {
        _httpClient = clientFactory.CreateClient();
        _baseUrl = settings.Value.BaseUrl;
        _httpClient.BaseAddress = new Uri(_baseUrl);
    }

    public async Task<List<RoomDto>> GetAllAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<RoomDto>>("Room/GetAll");
        return response ?? new List<RoomDto>();
    }

    public async Task<RoomDto?> GetByIdAsync(uint id)
    {
        return await _httpClient.GetFromJsonAsync<RoomDto>($"Room/GetById?id={id}");
    }

    public async Task<RoomDto> CreateAsync(CreateRoomDto dto)
    {
        var request = new { room_number = dto.room_number, status_id = dto.status_id, category_id = dto.category_id, nightly_rate = dto.nightly_rate, floor_number = dto.floor_number };
        var response = await _httpClient.PostAsJsonAsync("Room/Create", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RoomDto>() ?? throw new Exception("Create failed");
    }

    public async Task<RoomDto> UpdateAsync(RoomDto dto)
    {
        var request = new { id = dto.id, room_number = dto.room_number, floor_number = dto.floor_number, nightly_rate = dto.nightly_rate, status_id = dto.status_id, category_id = dto.category_id };
        var response = await _httpClient.PutAsJsonAsync("Room/Update", request);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var updatedRoom = JsonSerializer.Deserialize<RoomDto>(responseContent);
        return updatedRoom ?? throw new Exception($"Update failed: received an empty or invalid response. Status code: {(int)response.StatusCode} ({response.StatusCode}). Response content: {responseContent}");
    }

    public async Task DeleteAsync(uint id)
    {
        var response = await _httpClient.DeleteAsync($"Room/Delete?id={id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> RoomNumberExistsAsync(uint roomNumber, uint? excludeId = null)
    {
        var all = await GetAllAsync();
        return all.Any(r => r.room_number == roomNumber && (excludeId == null || r.id != excludeId.Value));
    }

    public async Task<RoomDto?> GetByRoomNumberAsync(uint roomNumber)
    {
        return await _httpClient.GetFromJsonAsync<RoomDto>($"Room/GetByRoomNumber?roomNumber={roomNumber}");
    }

    public async Task<List<RoomDto>> GetByNightlyRateRangeAsync(decimal minRate, decimal maxRate)
    {
        return await _httpClient.GetFromJsonAsync<List<RoomDto>>($"Room/GetByNightlyRateRange?minRate={minRate}&maxRate={maxRate}") ?? new();
    }

    public async Task<List<BookingDto>> GetBookingsByRoomIdAsync(uint roomId)
    {
        var response = await _httpClient.GetFromJsonAsync<List<BookingDto>>($"Room/GetBookingsByRoomId?roomId={roomId}");
        return response ?? new();
    }

    public async Task<List<BookingHistoryDto>> GetBookingHistoriesByRoomIdAsync(uint roomId)
    {
        var response = await _httpClient.GetFromJsonAsync<List<BookingHistoryDto>>($"Room/GetBookingHistoriesByRoomId?roomId={roomId}");
        return response ?? new();
    }

    public async Task<List<RoomAvailabilityDto>> GetAvailabilitiesByRoomIdAsync(uint roomId)
    {
        var response = await _httpClient.GetFromJsonAsync<List<RoomAvailabilityDto>>($"Room/GetAvailabilitiesByRoomId?roomId={roomId}");
        return response ?? new();
    }

}
