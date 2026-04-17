using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace HotelCarga.Web.Services;

public record UserSummaryDto(uint id, string username, string email, string? role, string? status, uint? customer);

public record UserRoleDto(byte id, string role_name, string? description, DateTime? created_at);

public record UserStatusDto(byte id, string status_name, string? description, DateTime? created_at);

public record CustomerReferenceDto(
    uint id,
    uint user_id,
    string? document_number,
    string? first_name,
    string? last_name,
    string? phone,
    string? address,
    string? city,
    string? country,
    DateTime? created_at,
    DateTime? updated_at);

public record UserEntityDto(
    uint id,
    string username,
    string email,
    string password_hash,
    byte role_id,
    byte status_id,
    DateTime? created_at,
    DateTime? updated_at,
    UserRoleDto? role,
    UserStatusDto? status,
    CustomerReferenceDto? customer);

public record SaveUserDto(string username, string email, string password_hash, byte role_id, byte status_id);

public record UserMutationResponse(uint id, string? message, string? username, string? email, byte? role_id, byte? status_id);

public class ApiRequestException : HttpRequestException
{
    public HttpStatusCode HttpStatus { get; }
    public string? ResponseBody { get; }

    public ApiRequestException(HttpStatusCode statusCode, string message, string? responseBody = null)
        : base(message, null, statusCode)
    {
        HttpStatus = statusCode;
        ResponseBody = responseBody;
    }
}

public interface IUserApiService
{
    Task<List<UserSummaryDto>> GetAllAsync();
    Task<UserSummaryDto?> GetByIdAsync(uint id);
    Task<uint> CreateAsync(SaveUserDto dto);
    Task UpdateAsync(uint id, SaveUserDto dto);
    Task DeleteAsync(uint id);
    Task<string?> GetUsernameByIdAsync(uint id);
    Task<UserEntityDto?> GetUserWithRoleByUsernameAsync(string username);
    Task<UserEntityDto?> GetUserWithRoleByEmailAsync(string email);
    Task<UserEntityDto?> GetEditableByIdAsync(uint id);
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
    Task<List<UserRoleDto>> GetRolesAsync();
    Task<List<UserStatusDto>> GetStatusesAsync();
}

public class UserApiService : IUserApiService
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public UserApiService(IHttpClientFactory clientFactory, IOptions<ApiSettings> settings)
    {
        _httpClient = clientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(settings.Value.BaseUrl);
    }

    public async Task<List<UserSummaryDto>> GetAllAsync()
    {
        return await GetRequiredAsync<List<UserSummaryDto>>("User/GetAll") ?? [];
    }

    public async Task<UserSummaryDto?> GetByIdAsync(uint id)
    {
        return await GetOptionalAsync<UserSummaryDto>($"User/GetById?id={id}");
    }

    public async Task<uint> CreateAsync(SaveUserDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("User/Create", dto);
        await EnsureSuccessWithDetailsAsync(response, "POST User/Create");

        var payload = await response.Content.ReadFromJsonAsync<UserMutationResponse>(JsonOptions)
            ?? throw new InvalidOperationException("User create response was empty.");

        return payload.id;
    }

    public async Task UpdateAsync(uint id, SaveUserDto dto)
    {
        var request = new
        {
            id,
            dto.username,
            dto.email,
            dto.password_hash,
            dto.role_id,
            dto.status_id
        };

        var response = await _httpClient.PutAsJsonAsync("User/Update", request);
        await EnsureSuccessWithDetailsAsync(response, "PUT User/Update");
    }

    public async Task DeleteAsync(uint id)
    {
        var response = await _httpClient.DeleteAsync($"User/Delete?id={id}");
        await EnsureSuccessWithDetailsAsync(response, $"DELETE User/Delete?id={id}");
    }

    public async Task<string?> GetUsernameByIdAsync(uint id)
    {
        return await GetOptionalAsync<string>($"User/GetUsernameById?id={id}");
    }

    public async Task<UserEntityDto?> GetUserWithRoleByUsernameAsync(string username)
    {
        return await GetOptionalAsync<UserEntityDto>($"User/GetUserWithRoleByUsername?username={Uri.EscapeDataString(username)}");
    }

    public async Task<UserEntityDto?> GetUserWithRoleByEmailAsync(string email)
    {
        return await GetOptionalAsync<UserEntityDto>($"User/GetUserWithRoleByEmail?email={Uri.EscapeDataString(email)}");
    }

    public async Task<UserEntityDto?> GetEditableByIdAsync(uint id)
    {
        var summary = await GetByIdAsync(id);
        if (summary is null)
        {
            return null;
        }

        return await GetUserWithRoleByUsernameAsync(summary.username);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await GetRequiredAsync<bool>($"User/UsernameExists?username={Uri.EscapeDataString(username)}");
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await GetRequiredAsync<bool>($"User/EmailExists?email={Uri.EscapeDataString(email)}");
    }

    public async Task<List<UserRoleDto>> GetRolesAsync()
    {
        return await GetRequiredAsync<List<UserRoleDto>>("Role/GetAll") ?? [];
    }

    public async Task<List<UserStatusDto>> GetStatusesAsync()
    {
        return await GetRequiredAsync<List<UserStatusDto>>("UserStatus/GetAll") ?? [];
    }

    private async Task<T?> GetOptionalAsync<T>(string url)
    {
        var response = await _httpClient.GetAsync(url);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }

        await EnsureSuccessWithDetailsAsync(response, $"GET {url}");
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    private async Task<T> GetRequiredAsync<T>(string url)
    {
        var response = await _httpClient.GetAsync(url);
        await EnsureSuccessWithDetailsAsync(response, $"GET {url}");

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions)
            ?? throw new InvalidOperationException($"A required response for '{url}' was empty.");
    }

    private static async Task EnsureSuccessWithDetailsAsync(HttpResponseMessage response, string operation)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = response.Content is null ? null : await response.Content.ReadAsStringAsync();
        var trimmedBody = string.IsNullOrWhiteSpace(body)
            ? null
            : body.Length > 500
                ? body[..500]
                : body;

        throw new ApiRequestException(
            response.StatusCode,
            $"{operation} failed with {(int)response.StatusCode} ({response.ReasonPhrase}).",
            trimmedBody);
    }
}