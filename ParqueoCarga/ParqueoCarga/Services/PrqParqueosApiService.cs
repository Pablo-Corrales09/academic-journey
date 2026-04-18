using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ParqueoCarga.Configuration;
using ParqueoCarga.ViewModels.Parqueos;

namespace ParqueoCarga.Services;

public sealed class PrqParqueosApiService : IPrqParqueosApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ApiSettings _apiSettings;

    public PrqParqueosApiService(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
    {
        _httpClient = httpClient;
        _apiSettings = apiSettings.Value;
    }

    public Task<IReadOnlyList<ParqueoViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
        => GetListAsync("api/prq-parqueos", cancellationToken);

    public async Task<ParqueoViewModel?> GetByIdAsync(uint id, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        using var response = await _httpClient.GetAsync($"api/prq-parqueos/{id}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response, cancellationToken);
        return await DeserializeAsync<ParqueoViewModel>(response, cancellationToken);
    }

    public async Task<ParqueoViewModel> CreateAsync(ParqueoUpsertViewModel request, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        using var response = await _httpClient.PostAsync(
            "api/prq-parqueos",
            CreateJsonContent(request),
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
        return (await DeserializeAsync<ParqueoViewModel>(response, cancellationToken))!;
    }

    public async Task UpdateAsync(uint id, ParqueoUpsertViewModel request, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        using var response = await _httpClient.PutAsync(
            $"api/prq-parqueos/{id}",
            CreateJsonContent(request),
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DeleteAsync(uint id, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        using var response = await _httpClient.DeleteAsync($"api/prq-parqueos/{id}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private async Task<IReadOnlyList<ParqueoViewModel>> GetListAsync(string requestUri, CancellationToken cancellationToken)
    {
        EnsureConfigured();

        using var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await DeserializeAsync<List<ParqueoViewModel>>(response, cancellationToken) ?? [];
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_apiSettings.BaseUrl))
        {
            throw new InvalidOperationException("Configura ApiSettings:BaseUrl en appsettings.json para consumir el API de Parqueos.");
        }
    }

    private static StringContent CreateJsonContent<TValue>(TValue value)
        => new(JsonSerializer.Serialize(value, JsonOptions), Encoding.UTF8, "application/json");

    private static async Task<T?> DeserializeAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content.Headers.ContentLength == 0)
        {
            return default;
        }

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<T>(contentStream, JsonOptions, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var defaultMessage = "El API de Parqueos no pudo procesar la solicitud.";
        var payload = response.Content is null
            ? string.Empty
            : await response.Content.ReadAsStringAsync(cancellationToken);

        IReadOnlyDictionary<string, string[]>? errors = null;
        var message = defaultMessage;

        if (!string.IsNullOrWhiteSpace(payload))
        {
            try
            {
                var validationProblem = JsonSerializer.Deserialize<ValidationProblemDetails>(payload, JsonOptions);
                if (validationProblem?.Errors?.Count > 0)
                {
                    errors = new Dictionary<string, string[]>(validationProblem.Errors, StringComparer.OrdinalIgnoreCase);
                    message = validationProblem.Title ?? "La solicitud enviada al API contiene errores de validación.";
                }
            }
            catch (JsonException)
            {
            }

            if (errors is null)
            {
                try
                {
                    var problem = JsonSerializer.Deserialize<ProblemDetails>(payload, JsonOptions);
                    if (problem is not null)
                    {
                        message = problem.Detail ?? problem.Title ?? defaultMessage;
                    }
                    else if (!string.IsNullOrWhiteSpace(payload))
                    {
                        message = payload;
                    }
                }
                catch (JsonException)
                {
                    message = payload;
                }
            }
        }

        throw new ApiServiceException(message, response.StatusCode, errors);
    }
}
