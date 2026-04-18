using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ParqueoCarga.Configuration;
using ParqueoCarga.ViewModels.IngresosAutomovil;

namespace ParqueoCarga.Services;

public sealed class PrqIngresosAutomovilApiService : IPrqIngresosAutomovilApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ApiSettings _apiSettings;

    public PrqIngresosAutomovilApiService(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
    {
        _httpClient = httpClient;
        _apiSettings = apiSettings.Value;
    }

    public Task<IReadOnlyList<IngresoAutomovilViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
        => GetListAsync("api/prq-ingresos-automovil", cancellationToken);

    public async Task<IngresoAutomovilViewModel?> GetByIdAsync(uint consecutivo, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        using var response = await _httpClient.GetAsync($"api/prq-ingresos-automovil/{consecutivo}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response, cancellationToken);
        return await DeserializeAsync<IngresoAutomovilViewModel>(response, cancellationToken);
    }

    public async Task<IngresoAutomovilViewModel> CreateAsync(IngresoAutomovilUpsertViewModel request, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        using var response = await _httpClient.PostAsync(
            "api/prq-ingresos-automovil",
            CreateJsonContent(request),
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
        return (await DeserializeAsync<IngresoAutomovilViewModel>(response, cancellationToken))!;
    }

    public async Task UpdateAsync(uint consecutivo, IngresoAutomovilUpsertViewModel request, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        using var response = await _httpClient.PutAsync(
            $"api/prq-ingresos-automovil/{consecutivo}",
            CreateJsonContent(request),
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DeleteAsync(uint consecutivo, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        using var response = await _httpClient.DeleteAsync($"api/prq-ingresos-automovil/{consecutivo}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<decimal?> ObtenerPrecioPorHoraAsync(uint idParqueo, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        using var response = await _httpClient.GetAsync($"api/prq-ingresos-automovil/precio-por-hora/{idParqueo}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response, cancellationToken);

        if (response.Content.Headers.ContentLength == 0)
        {
            return null;
        }

        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        if (decimal.TryParse(payload, NumberStyles.Any, CultureInfo.InvariantCulture, out var directValue))
        {
            return directValue;
        }

        return JsonSerializer.Deserialize<decimal?>(payload, JsonOptions);
    }

    public Task<IReadOnlyList<IngresoAutomovilViewModel>> GetByTipoAndDateRangeAsync(
        string tipo,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var requestUri =
            $"api/prq-ingresos-automovil/por-tipo-y-fecha?tipo={Uri.EscapeDataString(tipo)}&startDate={Uri.EscapeDataString(startDate.ToString("O", CultureInfo.InvariantCulture))}&endDate={Uri.EscapeDataString(endDate.ToString("O", CultureInfo.InvariantCulture))}";

        return GetListAsync(requestUri, cancellationToken);
    }

    public Task<IReadOnlyList<IngresoAutomovilViewModel>> GetByProvinciaAndDateRangeAsync(
        string provincia,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var requestUri =
            $"api/prq-ingresos-automovil/por-provincia-y-fecha?provincia={Uri.EscapeDataString(provincia)}&startDate={Uri.EscapeDataString(startDate.ToString("O", CultureInfo.InvariantCulture))}&endDate={Uri.EscapeDataString(endDate.ToString("O", CultureInfo.InvariantCulture))}";

        return GetListAsync(requestUri, cancellationToken);
    }

    private async Task<IReadOnlyList<IngresoAutomovilViewModel>> GetListAsync(string requestUri, CancellationToken cancellationToken)
    {
        EnsureConfigured();

        using var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await DeserializeAsync<List<IngresoAutomovilViewModel>>(response, cancellationToken) ?? [];
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_apiSettings.BaseUrl))
        {
            throw new InvalidOperationException("Configura ApiSettings:BaseUrl en appsettings.json para consumir el API de Ingresos de Automóvil.");
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

        var defaultMessage = "El API de Ingresos de Automóvil no pudo procesar la solicitud.";
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
