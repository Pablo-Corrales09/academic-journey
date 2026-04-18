using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ParqueoCarga.Configuration;
using ParqueoCarga.ViewModels.Automoviles;

namespace ParqueoCarga.Services;

public sealed class PrqAutomovilesApiService : IPrqAutomovilesApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ApiSettings _apiSettings;

    public PrqAutomovilesApiService(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
    {
        _httpClient = httpClient;
        _apiSettings = apiSettings.Value;
    }

    public Task<IReadOnlyList<AutomovilViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
        => GetListAsync("api/prq-automoviles", cancellationToken);

    public async Task<AutomovilViewModel?> GetByIdAsync(uint id, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        using var response = await _httpClient.GetAsync($"api/prq-automoviles/{id}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response, cancellationToken);
        return await DeserializeAsync<AutomovilViewModel>(response, cancellationToken);
    }

    public async Task<AutomovilViewModel> CreateAsync(AutomovilUpsertViewModel request, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        using var response = await _httpClient.PostAsync(
            "api/prq-automoviles",
            CreateJsonContent(request),
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
        return (await DeserializeAsync<AutomovilViewModel>(response, cancellationToken))!;
    }

    public async Task UpdateAsync(uint id, AutomovilUpsertViewModel request, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        using var response = await _httpClient.PutAsync(
            $"api/prq-automoviles/{id}",
            CreateJsonContent(request),
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DeleteAsync(uint id, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        using var response = await _httpClient.DeleteAsync($"api/prq-automoviles/{id}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public Task<IReadOnlyList<AutomovilViewModel>> GetByColorAsync(string color, CancellationToken cancellationToken = default)
        => GetListAsync($"api/prq-automoviles/por-color?color={Uri.EscapeDataString(color)}", cancellationToken);

    public Task<IReadOnlyList<AutomovilViewModel>> GetByYearRangeAsync(short startYear, short endYear, CancellationToken cancellationToken = default)
        => GetListAsync($"api/prq-automoviles/por-anio?startYear={startYear}&endYear={endYear}", cancellationToken);

    public Task<IReadOnlyList<AutomovilViewModel>> GetByFabricanteAsync(string fabricante, CancellationToken cancellationToken = default)
        => GetListAsync($"api/prq-automoviles/por-fabricante?fabricante={Uri.EscapeDataString(fabricante)}", cancellationToken);

    public Task<IReadOnlyList<AutomovilViewModel>> GetByTipoAsync(string tipo, CancellationToken cancellationToken = default)
        => GetListAsync($"api/prq-automoviles/por-tipo?tipo={Uri.EscapeDataString(tipo)}", cancellationToken);

    private async Task<IReadOnlyList<AutomovilViewModel>> GetListAsync(string requestUri, CancellationToken cancellationToken)
    {
        EnsureConfigured();

        using var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await DeserializeAsync<List<AutomovilViewModel>>(response, cancellationToken) ?? [];
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_apiSettings.BaseUrl))
        {
            throw new InvalidOperationException("Configura ApiSettings:BaseUrl en appsettings.json para consumir el API de Automóviles.");
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

        var defaultMessage = "El API de Automóviles no pudo procesar la solicitud.";
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