using System.Net;
using Microsoft.AspNetCore.Mvc;
using ParqueoCarga.Services;
using ParqueoCarga.ViewModels.IngresosAutomovil;

namespace ParqueoCarga.Controllers;

[Route("ingresos-automovil")]
public sealed class IngresosAutomovilController : Controller
{
    private readonly IPrqIngresosAutomovilApiService _ingresosApiService;
    private readonly ILogger<IngresosAutomovilController> _logger;

    public IngresosAutomovilController(
        IPrqIngresosAutomovilApiService ingresosApiService,
        ILogger<IngresosAutomovilController> logger)
    {
        _ingresosApiService = ingresosApiService;
        _logger = logger;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        return View(new IngresosAutomovilIndexViewModel());
    }

    [HttpGet("listado")]
    public async Task<IActionResult> Listado([FromQuery] IngresoAutomovilFilterViewModel filtros, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (filtros.FechaInicio.HasValue && filtros.FechaFin.HasValue && filtros.FechaInicio > filtros.FechaFin)
        {
            ModelState.AddModelError(nameof(filtros.FechaFin), "La fecha final debe ser mayor o igual a la fecha inicial.");
            return ValidationProblem(ModelState);
        }

        try
        {
            var ingresos = await ResolveIngresosAsync(filtros, cancellationToken);
            var filtered = ApplyClientFiltering(ingresos, filtros);

            return Ok(new
            {
                items = filtered,
                total = filtered.Count,
                stats = new
                {
                    total = filtered.Count,
                    activos = filtered.Count(item => item.FechaSalida is null),
                    montoAcumulado = filtered.Sum(item => item.MontoTotalPagar ?? 0m)
                }
            });
        }
        catch (ApiServiceException exception)
        {
            return CreateApiErrorResult(exception);
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogWarning(exception, "La integración con el API de Ingresos no está configurada correctamente.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = exception.Message });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error inesperado al obtener Ingresos de Automóvil.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "No fue posible cargar los ingresos." });
        }
    }

    [HttpGet("{consecutivo:long}")]
    public async Task<IActionResult> Obtener(uint consecutivo, CancellationToken cancellationToken)
    {
        try
        {
            var ingreso = await _ingresosApiService.GetByIdAsync(consecutivo, cancellationToken);
            return ingreso is null
                ? NotFound(new { message = "El ingreso solicitado no existe." })
                : Ok(ingreso);
        }
        catch (ApiServiceException exception)
        {
            return CreateApiErrorResult(exception);
        }
        catch (InvalidOperationException exception)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = exception.Message });
        }
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear([FromBody] IngresoAutomovilUpsertViewModel request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (request.FechaSalida.HasValue && request.FechaSalida.Value < request.FechaEntrada)
        {
            ModelState.AddModelError(nameof(request.FechaSalida), "La fecha de salida no puede ser menor que la fecha de entrada.");
            return ValidationProblem(ModelState);
        }

        try
        {
            var created = await _ingresosApiService.CreateAsync(request, cancellationToken);
            return Ok(new { message = "Ingreso creado correctamente.", item = created });
        }
        catch (ApiServiceException exception)
        {
            return CreateApiErrorResult(exception);
        }
        catch (InvalidOperationException exception)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = exception.Message });
        }
    }

    [HttpPut("{consecutivo:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(uint consecutivo, [FromBody] IngresoAutomovilUpsertViewModel request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (request.FechaSalida.HasValue && request.FechaSalida.Value < request.FechaEntrada)
        {
            ModelState.AddModelError(nameof(request.FechaSalida), "La fecha de salida no puede ser menor que la fecha de entrada.");
            return ValidationProblem(ModelState);
        }

        try
        {
            await _ingresosApiService.UpdateAsync(consecutivo, request, cancellationToken);
            var updated = await _ingresosApiService.GetByIdAsync(consecutivo, cancellationToken);

            return Ok(new { message = "Ingreso actualizado correctamente.", item = updated });
        }
        catch (ApiServiceException exception)
        {
            return CreateApiErrorResult(exception);
        }
        catch (InvalidOperationException exception)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = exception.Message });
        }
    }

    [HttpDelete("{consecutivo:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(uint consecutivo, CancellationToken cancellationToken)
    {
        try
        {
            await _ingresosApiService.DeleteAsync(consecutivo, cancellationToken);
            return Ok(new { message = "Ingreso eliminado correctamente." });
        }
        catch (ApiServiceException exception)
        {
            return CreateApiErrorResult(exception);
        }
        catch (InvalidOperationException exception)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = exception.Message });
        }
    }

    [HttpGet("precio-por-hora/{idParqueo:long}")]
    public async Task<IActionResult> ObtenerPrecioPorHora(uint idParqueo, CancellationToken cancellationToken)
    {
        try
        {
            var precio = await _ingresosApiService.ObtenerPrecioPorHoraAsync(idParqueo, cancellationToken);
            return precio is null
                ? NotFound(new { message = "No se encontró precio por hora para el parqueo indicado." })
                : Ok(new { idParqueo, precioHora = precio.Value });
        }
        catch (ApiServiceException exception)
        {
            return CreateApiErrorResult(exception);
        }
        catch (InvalidOperationException exception)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = exception.Message });
        }
    }

    private async Task<IReadOnlyList<IngresoAutomovilViewModel>> ResolveIngresosAsync(
        IngresoAutomovilFilterViewModel filtros,
        CancellationToken cancellationToken)
    {
        var hasSearch = !string.IsNullOrWhiteSpace(filtros.Termino);
        var hasDateRange = filtros.FechaInicio.HasValue && filtros.FechaFin.HasValue;

        if (hasSearch && hasDateRange)
        {
            var term = filtros.Termino!.Trim();
            return filtros.Criterio.ToLowerInvariant() switch
            {
                "tipo" => await _ingresosApiService.GetByTipoAndDateRangeAsync(term, filtros.FechaInicio!.Value, filtros.FechaFin!.Value, cancellationToken),
                "provincia" => await _ingresosApiService.GetByProvinciaAndDateRangeAsync(term, filtros.FechaInicio!.Value, filtros.FechaFin!.Value, cancellationToken),
                _ => await _ingresosApiService.GetAllAsync(cancellationToken)
            };
        }

        return await _ingresosApiService.GetAllAsync(cancellationToken);
    }

    private static List<IngresoAutomovilViewModel> ApplyClientFiltering(
        IEnumerable<IngresoAutomovilViewModel> ingresos,
        IngresoAutomovilFilterViewModel filtros)
    {
        var query = ingresos;

        if (filtros.FechaInicio.HasValue)
        {
            query = query.Where(item => item.FechaEntrada >= filtros.FechaInicio.Value);
        }

        if (filtros.FechaFin.HasValue)
        {
            query = query.Where(item => item.FechaEntrada <= filtros.FechaFin.Value);
        }

        if (filtros.Criterio.Equals("todos", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(filtros.Termino))
        {
            var term = filtros.Termino.Trim();
            query = query.Where(item =>
                item.Consecutivo.ToString().Contains(term, StringComparison.OrdinalIgnoreCase) ||
                item.IdParqueo.ToString().Contains(term, StringComparison.OrdinalIgnoreCase) ||
                item.IdAutomovil.ToString().Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        query = filtros.Orden.Equals("asc", StringComparison.OrdinalIgnoreCase)
            ? query.OrderBy(item => item.FechaEntrada).ThenBy(item => item.Consecutivo)
            : query.OrderByDescending(item => item.FechaEntrada).ThenBy(item => item.Consecutivo);

        return query.ToList();
    }

    private IActionResult CreateApiErrorResult(ApiServiceException exception)
    {
        if (exception.StatusCode == HttpStatusCode.BadRequest && exception.Errors is not null)
        {
            foreach (var error in exception.Errors)
            {
                foreach (var message in error.Value)
                {
                    ModelState.AddModelError(error.Key, message);
                }
            }

            return ValidationProblem(ModelState);
        }

        var statusCode = exception.StatusCode switch
        {
            HttpStatusCode.NotFound => StatusCodes.Status404NotFound,
            HttpStatusCode.BadRequest => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status502BadGateway
        };

        _logger.LogWarning("Error devuelto por API de Ingresos. Status: {StatusCode}. Mensaje: {Message}", statusCode, exception.Message);
        return StatusCode(statusCode, new { message = exception.Message });
    }
}
