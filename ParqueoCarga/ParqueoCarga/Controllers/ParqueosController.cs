using System.Net;
using Microsoft.AspNetCore.Mvc;
using ParqueoCarga.Services;
using ParqueoCarga.ViewModels.Parqueos;

namespace ParqueoCarga.Controllers;

[Route("parqueos")]
public sealed class ParqueosController : Controller
{
    private readonly IPrqParqueosApiService _parqueosApiService;
    private readonly ILogger<ParqueosController> _logger;

    public ParqueosController(
        IPrqParqueosApiService parqueosApiService,
        ILogger<ParqueosController> logger)
    {
        _parqueosApiService = parqueosApiService;
        _logger = logger;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        return View(new ParqueosIndexViewModel());
    }

    [HttpGet("listado")]
    public async Task<IActionResult> Listado(
        [FromQuery] string? provincia,
        [FromQuery] string? nombre,
        [FromQuery] string orden = "asc",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var parqueos = await _parqueosApiService.GetAllAsync(cancellationToken);
            var query = parqueos.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(provincia))
            {
                query = query.Where(item =>
                    item.Provincia.Contains(provincia.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                query = query.Where(item =>
                    item.Nombre.Contains(nombre.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            query = orden.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderByDescending(item => item.PrecioHora).ThenBy(item => item.Nombre)
                : query.OrderBy(item => item.PrecioHora).ThenBy(item => item.Nombre);

            var filtered = query.ToList();

            return Ok(new
            {
                items = filtered,
                total = filtered.Count,
                stats = new
                {
                    total = filtered.Count,
                    precioMinimo = filtered.Count == 0 ? (decimal?)null : filtered.Min(item => item.PrecioHora),
                    precioMaximo = filtered.Count == 0 ? (decimal?)null : filtered.Max(item => item.PrecioHora),
                    provincias = filtered.Select(item => item.Provincia).Distinct(StringComparer.OrdinalIgnoreCase).Count()
                }
            });
        }
        catch (ApiServiceException exception)
        {
            return CreateApiErrorResult(exception);
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogWarning(exception, "La integración con el API de Parqueos no está configurada correctamente.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = exception.Message });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error inesperado al obtener Parqueos.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "No fue posible cargar los parqueos." });
        }
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Obtener(uint id, CancellationToken cancellationToken)
    {
        try
        {
            var parqueo = await _parqueosApiService.GetByIdAsync(id, cancellationToken);
            return parqueo is null
                ? NotFound(new { message = "El parqueo solicitado no existe." })
                : Ok(parqueo);
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
    public async Task<IActionResult> Crear([FromBody] ParqueoUpsertViewModel request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var created = await _parqueosApiService.CreateAsync(request, cancellationToken);
            return Ok(new { message = "Parqueo creado correctamente.", item = created });
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
    public async Task<IActionResult> Editar(uint id, [FromBody] ParqueoUpsertViewModel request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            await _parqueosApiService.UpdateAsync(id, request, cancellationToken);
            var updated = await _parqueosApiService.GetByIdAsync(id, cancellationToken);

            return Ok(new { message = "Parqueo actualizado correctamente.", item = updated });
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
            await _parqueosApiService.DeleteAsync(id, cancellationToken);
            return Ok(new { message = "Parqueo eliminado correctamente." });
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

        _logger.LogWarning("Error devuelto por API de Parqueos. Status: {StatusCode}. Mensaje: {Message}", statusCode, exception.Message);
        return StatusCode(statusCode, new { message = exception.Message });
    }
}
