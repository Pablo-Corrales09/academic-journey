using System.Net;
using Microsoft.AspNetCore.Mvc;
using ParqueoCarga.Services;
using ParqueoCarga.ViewModels.Automoviles;

namespace ParqueoCarga.Controllers;

[Route("automoviles")]
public sealed class AutomovilesController : Controller
{
    private readonly IPrqAutomovilesApiService _automovilesApiService;
    private readonly ILogger<AutomovilesController> _logger;

    public AutomovilesController(
        IPrqAutomovilesApiService automovilesApiService,
        ILogger<AutomovilesController> logger)
    {
        _automovilesApiService = automovilesApiService;
        _logger = logger;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        return View(new AutomovilesIndexViewModel
        {
            Filtros = new AutomovilFilterViewModel
            {
                Orden = "desc"
            }
        });
    }

    [HttpGet("listado")]
    public async Task<IActionResult> Listado([FromQuery] AutomovilFilterViewModel filtros, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (filtros.AnioInicio.HasValue && filtros.AnioFin.HasValue && filtros.AnioInicio > filtros.AnioFin)
        {
            ModelState.AddModelError(nameof(filtros.AnioFin), "El año final debe ser mayor o igual al año inicial.");
            return ValidationProblem(ModelState);
        }

        try
        {
            var automoviles = await ResolveAutomovilesAsync(filtros, cancellationToken);
            var filtered = ApplyClientFiltering(automoviles, filtros);

            return Ok(new
            {
                items = filtered,
                total = filtered.Count,
                stats = new
                {
                    total = filtered.Count,
                    modelosUnicos = filtered.Select(item => item.Fabricante).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                    anioMinimo = filtered.Count == 0 ? (short?)null : filtered.Min(item => item.Anio),
                    anioMaximo = filtered.Count == 0 ? (short?)null : filtered.Max(item => item.Anio)
                }
            });
        }
        catch (ApiServiceException exception)
        {
            return CreateApiErrorResult(exception);
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogWarning(exception, "La integración con el API de Automóviles no está configurada correctamente.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = exception.Message });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error inesperado al obtener Automóviles.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "No fue posible cargar los automóviles." });
        }
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Obtener(uint id, CancellationToken cancellationToken)
    {
        try
        {
            var automovil = await _automovilesApiService.GetByIdAsync(id, cancellationToken);
            return automovil is null
                ? NotFound(new { message = "El automóvil solicitado no existe." })
                : Ok(automovil);
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
    public async Task<IActionResult> Crear([FromBody] AutomovilUpsertViewModel request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var created = await _automovilesApiService.CreateAsync(request, cancellationToken);
            return Ok(new { message = "Automóvil creado correctamente.", item = created });
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

    [HttpPut("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(uint id, [FromBody] AutomovilUpsertViewModel request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            await _automovilesApiService.UpdateAsync(id, request, cancellationToken);
            var updated = await _automovilesApiService.GetByIdAsync(id, cancellationToken);

            return Ok(new { message = "Automóvil actualizado correctamente.", item = updated });
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

    [HttpDelete("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(uint id, CancellationToken cancellationToken)
    {
        try
        {
            await _automovilesApiService.DeleteAsync(id, cancellationToken);
            return Ok(new { message = "Automóvil eliminado correctamente." });
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

    private async Task<IReadOnlyList<AutomovilViewModel>> ResolveAutomovilesAsync(AutomovilFilterViewModel filtros, CancellationToken cancellationToken)
    {
        var hasSearch = !string.IsNullOrWhiteSpace(filtros.TerminoBusqueda);
        var hasFullYearRange = filtros.AnioInicio.HasValue && filtros.AnioFin.HasValue;

        if (hasSearch)
        {
            var term = filtros.TerminoBusqueda!.Trim();
            return filtros.CampoBusqueda.ToLowerInvariant() switch
            {
                "color" => await _automovilesApiService.GetByColorAsync(term, cancellationToken),
                "tipo" => await _automovilesApiService.GetByTipoAsync(term, cancellationToken),
                "fabricante" => await _automovilesApiService.GetByFabricanteAsync(term, cancellationToken),
                _ => await _automovilesApiService.GetAllAsync(cancellationToken)
            };
        }

        if (hasFullYearRange)
        {
            return await _automovilesApiService.GetByYearRangeAsync(filtros.AnioInicio!.Value, filtros.AnioFin!.Value, cancellationToken);
        }

        return await _automovilesApiService.GetAllAsync(cancellationToken);
    }

    private static List<AutomovilViewModel> ApplyClientFiltering(IEnumerable<AutomovilViewModel> automoviles, AutomovilFilterViewModel filtros)
    {
        var query = automoviles;

        if (filtros.CampoBusqueda.Equals("todos", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(filtros.TerminoBusqueda))
        {
            var term = filtros.TerminoBusqueda.Trim();
            query = query.Where(item =>
                item.Color.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                item.Fabricante.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                item.Tipo.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        if (filtros.AnioInicio.HasValue)
        {
            query = query.Where(item => item.Anio >= filtros.AnioInicio.Value);
        }

        if (filtros.AnioFin.HasValue)
        {
            query = query.Where(item => item.Anio <= filtros.AnioFin.Value);
        }

        query = filtros.Orden.Equals("asc", StringComparison.OrdinalIgnoreCase)
            ? query.OrderBy(item => item.Anio).ThenBy(item => item.Fabricante)
            : query.OrderByDescending(item => item.Anio).ThenBy(item => item.Fabricante);

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

        _logger.LogWarning("Error devuelto por API de Automóviles. Status: {StatusCode}. Mensaje: {Message}", statusCode, exception.Message);
        return StatusCode(statusCode, new { message = exception.Message });
    }
}
