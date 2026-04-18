using Microsoft.AspNetCore.Mvc;
using ParqueoCarga.ApiModel.Dtos;
using ParqueoCarga.DbModel.Models;
using ParqueolCarga.RepositoryModel.Interfaces;

namespace ParqueoCarga.ApiModel.Controllers;

/// <summary>
/// Manages parking lots (<c>PrqParqueo</c>).
/// </summary>
[ApiController]
[Route("api/prq-parqueos")]
[Produces("application/json")]
public sealed class PrqParqueosController : ControllerBase
{
    private readonly IPrqParqueoRepository _repository;

    /// <summary>Initialises the controller with the required repository.</summary>
    public PrqParqueosController(IPrqParqueoRepository repository)
    {
        _repository = repository;
    }

    // -------------------------------------------------------------------------
    // CRUD
    // -------------------------------------------------------------------------

    /// <summary>Returns all parking lots.</summary>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <response code="200">List of parking lots (may be empty).</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PrqParqueo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PrqParqueo>>> GetAll(
        CancellationToken cancellationToken)
    {
        var parqueos = await _repository.GetAllAsync(cancellationToken);
        return Ok(parqueos);
    }

    /// <summary>Returns a single parking lot by its ID.</summary>
    /// <param name="id">Primary key of the parking lot.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <response code="200">The requested parking lot.</response>
    /// <response code="404">Parking lot not found.</response>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(PrqParqueo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrqParqueo>> GetById(
        [FromRoute] uint id,
        CancellationToken cancellationToken)
    {
        var parqueo = await _repository.GetByIdAsync(id, cancellationToken);
        return parqueo is null ? NotFound() : Ok(parqueo);
    }

    /// <summary>Creates a new parking lot.</summary>
    /// <param name="request">Parking lot data.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <response code="201">The newly created parking lot.</response>
    /// <response code="400">Validation errors in the request body.</response>
    [HttpPost]
    [ProducesResponseType(typeof(PrqParqueo), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PrqParqueo>> Create(
        [FromBody] PrqParqueoRequest request,
        CancellationToken cancellationToken)
    {
        var entity = new PrqParqueo
        {
            Provincia = request.Provincia,
            Nombre = request.Nombre,
            PrecioHora = request.PrecioHora
        };

        var created = await _repository.CreateAsync(entity, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an existing parking lot.</summary>
    /// <param name="id">Primary key of the parking lot to update.</param>
    /// <param name="request">Updated parking lot data.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <response code="204">Update succeeded.</response>
    /// <response code="400">Validation errors in the request body.</response>
    /// <response code="404">Parking lot not found.</response>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] uint id,
        [FromBody] PrqParqueoRequest request,
        CancellationToken cancellationToken)
    {
        var entity = new PrqParqueo
        {
            Id = id,
            Provincia = request.Provincia,
            Nombre = request.Nombre,
            PrecioHora = request.PrecioHora
        };

        var updated = await _repository.UpdateAsync(entity, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Deletes a parking lot by its ID.</summary>
    /// <param name="id">Primary key of the parking lot to delete.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <response code="204">Deletion succeeded.</response>
    /// <response code="404">Parking lot not found.</response>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] uint id,
        CancellationToken cancellationToken)
    {
        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    // -------------------------------------------------------------------------
    // Custom queries
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns parking lots filtered by province, name, and price range.
    /// All parameters are optional; omit any to skip that filter.
    /// </summary>
    /// <param name="provincia">Province to filter by (partial or exact match).</param>
    /// <param name="nombre">Name to filter by (partial or exact match).</param>
    /// <param name="minPrice">Minimum hourly price (inclusive). Defaults to 0.</param>
    /// <param name="maxPrice">Maximum hourly price (inclusive). Defaults to <see cref="decimal.MaxValue"/>.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <response code="200">List of matching parking lots (may be empty).</response>
    [HttpGet("por-filtros")]
    [ProducesResponseType(typeof(IEnumerable<PrqParqueo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PrqParqueo>>> GetByFiltros(
        [FromQuery] string? provincia,
        [FromQuery] string? nombre,
        [FromQuery] decimal minPrice = 0m,
        [FromQuery] decimal maxPrice = decimal.MaxValue,
        CancellationToken cancellationToken = default)
    {
        var results = await _repository.GetByFiltrosAsync(
            provincia, nombre, minPrice, maxPrice, cancellationToken);

        return Ok(results);
    }
}
