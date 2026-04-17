using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace HotelCarga.Web.Services;

public record RoomCategoryDto(byte id, string category_name, string? description, string? amenities);

public interface IRoomCategoryApiService
{
    Task<List<RoomCategoryDto>> GetAllAsync();
}

public class RoomCategoryApiService : IRoomCategoryApiService
{
    private readonly HttpClient _httpClient;

    public RoomCategoryApiService(IHttpClientFactory clientFactory, IOptions<ApiSettings> settings)
    {
        _httpClient = clientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(settings.Value.BaseUrl);
    }

    public async Task<List<RoomCategoryDto>> GetAllAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<RoomCategoryDto>>("RoomCategory/GetAll");
        return response ?? [];
    }
}
