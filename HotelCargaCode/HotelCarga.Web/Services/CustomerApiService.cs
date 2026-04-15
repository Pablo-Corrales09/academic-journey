using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace HotelCarga.Web.Services;

public record CustomerSummaryDto(
    uint id,
    string first_name,
    string last_name,
    string document_number,
    string phone,
    string country,
    string city,
    string address,
    string? username,
    string? email,
    string? role,
    string? status,
    JsonElement bookings,
    JsonElement waiting_queues);

public record CustomerEntityDto(
    uint id,
    uint user_id,
    string document_number,
    string first_name,
    string last_name,
    string phone,
    string address,
    string city,
    string country,
    DateTime? created_at,
    DateTime? updated_at);

public record SaveCustomerDto(
    uint user_id,
    string document_number,
    string first_name,
    string last_name,
    string phone,
    string address,
    string city,
    string country);

public record CustomerUpdateEnvelope(bool error, string message, CustomerSummaryDto? data);

public interface ICustomerApiService
{
    Task<List<CustomerSummaryDto>> GetAllAsync();
    Task<CustomerSummaryDto?> GetByIdAsync(uint id);
    Task<CustomerEntityDto?> GetByUserIdAsync(uint userId);
    Task<CustomerEntityDto?> GetByDocumentNumberAsync(string documentNumber);
    Task<CustomerEntityDto?> GetByPhoneAsync(string phone);
    Task<CustomerEntityDto?> GetByDocumentNumberOrPhoneAsync(string documentNumber, string phone);
    Task<List<CustomerEntityDto>> SearchByNameAsync(string searchString);
    Task<uint> CreateAsync(SaveCustomerDto dto);
    Task UpdateAsync(uint id, SaveCustomerDto dto);
    Task DeleteAsync(uint id);
    Task<List<uint>> GetBookingIdsByCustomerIdAsync(uint customerId);
    Task<List<uint>> GetBookingHistoryIdsByCustomerIdAsync(uint customerId);
    Task<List<uint>> GetWaitingQueueIdsByCustomerIdAsync(uint customerId);
}

public class CustomerApiService : ICustomerApiService
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public CustomerApiService(IHttpClientFactory clientFactory, IOptions<ApiSettings> settings)
    {
        _httpClient = clientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(settings.Value.BaseUrl);
    }

    public async Task<List<CustomerSummaryDto>> GetAllAsync()
    {
        return await GetRequiredAsync<List<CustomerSummaryDto>>("Customer/GetAll") ?? [];
    }

    public async Task<CustomerSummaryDto?> GetByIdAsync(uint id)
    {
        return await GetOptionalAsync<CustomerSummaryDto>($"Customer/GetById?id={id}");
    }

    public async Task<CustomerEntityDto?> GetByUserIdAsync(uint userId)
    {
        return await GetOptionalAsync<CustomerEntityDto>($"Customer/GetByUserId?userId={userId}");
    }

    public async Task<CustomerEntityDto?> GetByDocumentNumberAsync(string documentNumber)
    {
        return await GetOptionalAsync<CustomerEntityDto>($"Customer/GetByDocumentNumber?documentNumber={Uri.EscapeDataString(documentNumber)}");
    }

    public async Task<CustomerEntityDto?> GetByPhoneAsync(string phone)
    {
        return await GetOptionalAsync<CustomerEntityDto>($"Customer/GetByPhone?phone={Uri.EscapeDataString(phone)}");
    }

    public async Task<CustomerEntityDto?> GetByDocumentNumberOrPhoneAsync(string documentNumber, string phone)
    {
        return await GetOptionalAsync<CustomerEntityDto>(
            $"Customer/GetByDocumentNumberOrPhone?documentNumber={Uri.EscapeDataString(documentNumber)}&phone={Uri.EscapeDataString(phone)}");
    }

    public async Task<List<CustomerEntityDto>> SearchByNameAsync(string searchString)
    {
        return await GetRequiredAsync<List<CustomerEntityDto>>($"Customer/SearchByName?searchString={Uri.EscapeDataString(searchString)}") ?? [];
    }

    public async Task<uint> CreateAsync(SaveCustomerDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("Customer/Create", dto);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<CustomerEntityDto>(JsonOptions)
            ?? throw new InvalidOperationException("Customer create response was empty.");
        return created.id;
    }

    public async Task UpdateAsync(uint id, SaveCustomerDto dto)
    {
        var request = new
        {
            id,
            dto.user_id,
            dto.document_number,
            dto.first_name,
            dto.last_name,
            dto.phone,
            dto.address,
            dto.city,
            dto.country
        };

        var response = await _httpClient.PutAsJsonAsync("Customer/Update", request);
        response.EnsureSuccessStatusCode();
        _ = await response.Content.ReadFromJsonAsync<CustomerUpdateEnvelope>(JsonOptions);
    }

    public async Task DeleteAsync(uint id)
    {
        var response = await _httpClient.DeleteAsync($"Customer/Delete?id={id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<uint>> GetBookingIdsByCustomerIdAsync(uint customerId)
    {
        return await GetRequiredAsync<List<uint>>($"Customer/GetBookingIdsByCustomerId?customerId={customerId}") ?? [];
    }

    public async Task<List<uint>> GetBookingHistoryIdsByCustomerIdAsync(uint customerId)
    {
        return await GetRequiredAsync<List<uint>>($"Customer/GetBookingHistoryIdsByCustomerId?customerId={customerId}") ?? [];
    }

    public async Task<List<uint>> GetWaitingQueueIdsByCustomerIdAsync(uint customerId)
    {
        return await GetRequiredAsync<List<uint>>($"Customer/GetWaitingQueueIdsByCustomerId?customerId={customerId}") ?? [];
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
}