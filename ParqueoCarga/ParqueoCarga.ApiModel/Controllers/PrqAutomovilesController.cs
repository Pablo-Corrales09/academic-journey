using Microsoft.AspNetCore.Mvc;
using ParqueoCarga.ApiModel.Dtos;
using ParqueoCarga.DbModel.Models;
using ParqueolCarga.RepositoryModel.Interfaces;

namespace ParqueoCarga.ApiModel.Controllers;

/// <summary>
/// Manages vehicles (<c>PrqAutomovil</c>).
/// </summary>
[ApiController]
[Route("api/prq-automoviles")]
[Produces("application/json")]
public sealed class PrqAutomovilesController : ControllerBase
{
    private readonly IPrqAutomovilRepository _repository;

    /// <summary>Initialises the controller with the required repository.</summary>
    public PrqAutomovilesController(IPrqAutomovilRepository repository)
    {
        _repository = repository;
    }

    // -------------------------------------------------------------------------
    // CRUD
    // -------------------------------------------------------------------------

    /// <summary>Returns all vehicles.</summary>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <response code="200">List of vehicles (may be empty).</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PrqAutomovil>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PrqAutomovil>>> GetAll(
        CancellationToken cancellationToken)
    {
        var automoviles = await _repository.GetAllAsync(cancellationToken);
        return Ok(automoviles);
    }

    /// <summary>Returns a single vehicle by its ID.</summary>
    /// <param name="id">Primary key of the vehicle.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <response code="200">The requested vehicle.</response>
    /// <response code="404">Vehicle not found.</response>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(PrqAutomovil), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrqAutomovil>> GetById(
        [FromRoute] uint id,
        CancellationToken cancellationToken)
    {
        var automovil = await _repository.GetByIdAsync(id, cancellationToken);
        return automovil is null ? NotFound() : Ok(automovil);
    }

    /// <summary>Creates a new vehicle.</summary>
    /// <param name="request">Vehicle data.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <response code="201">The newly created vehicle.</response>
    /// <response code="400">Validation errors in the request body.</response>
    [HttpPost]
    [ProducesResponseType(typeof(PrqAutomovil), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PrqAutomovil>> Create(
        [FromBody] PrqAutomovilRequest request,
        CancellationToken cancellationToken)
    {
        var entity = new PrqAutomovil
        {
            Color = request.Color,
            Anio = request.Anio,
            Fabricante = request.Fabricante,
            Tipo = request.Tipo
        };

        var created = await _repository.CreateAsync(entity, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an existing vehicle.</summary>
    /// <param name="id">Primary key of the vehicle to update.</param>
    /// <param name="request">Updated vehicle data.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <response code="204">Update succeeded.</response>
    /// <response code="400">Validation errors in the request body.</response>
    /// <response code="404">Vehicle not found.</response>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] uint id,
        [FromBody] PrqAutomovilRequest request,
        CancellationToken cancellationToken)
    {
        var entity = new PrqAutomovil
        {
            Id = id,
            Color = request.Color,
            Anio = request.Anio,
            Fabricante = request.Fabricante,
            Tipo = request.Tipo
        };

        var updated = await _repository.UpdateAsync(entity, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Deletes a vehicle by its ID.</summary>
    /// <param name="id">Primary key of the vehicle to delete.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <response code="204">Deletion succeeded.</response>
    /// <response code="404">Vehicle not found.</response>
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

    /// <summary>Returns vehicles whose colour contains the specified text (case-insensitive).</summary>
    /// <param name="color">Partial or exact colour string to search for.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <response code="200">List of matching vehicles (may be empty).</response>
    [HttpGet("por-color")]
    [ProducesResponseType(typeof(IEnumerable<PrqAutomovil>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PrqAutomovil>>> GetByPartialColor(
        [FromQuery] string color,
        CancellationToken cancellationToken)
    {
        var results = await _repository.GetByPartialColorAsync(color, cancellationToken);
        return Ok(results);
    }

    /// <summary>Returns vehicles whose manufacturing year falls within the given range (inclusive).</summary>
    /// <param name="startYear">Start of the year range.</param>
    /// <param name="endYear">End of the year range.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <response code="200">List of matching vehicles (may be empty).</response>
    /// <response code="400"><paramref name="startYear"/> is greater than <paramref name="endYear"/>.</response>
    [HttpGet("por-anio")]
    [ProducesResponseType(typeof(IEnumerable<PrqAutomovil>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<PrqAutomovil>>> GetByYearRange(
        [FromQuery] int startYear,
        [FromQuery] int endYear,
        CancellationToken cancellationToken)
    {
        if (startYear > endYear)
            return BadRequest("startYear must be less than or equal to endYear.");

        var results = await _repository.GetByYearRangeAsync(startYear, endYear, cancellationToken);
        return Ok(results);
    }

    /// <summary>Returns vehicles whose manufacturer name contains the specified text (case-insensitive).</summary>
    /// <param name="fabricante">Partial or exact manufacturer name to search for.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <response code="200">List of matching vehicles (may be empty).</response>
    [HttpGet("por-fabricante")]
    [ProducesResponseType(typeof(IEnumerable<PrqAutomovil>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PrqAutomovil>>> GetByPartialFabricante(
        [FromQuery] string fabricante,
        CancellationToken cancellationToken)
    {
        var results = await _repository.GetByPartialFabricanteAsync(fabricante, cancellationToken);
        return Ok(results);
    }

    /// <summary>Returns vehicles whose type contains the specified text (case-insensitive).</summary>
    /// <param name="tipo">Partial or exact vehicle type to search for.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <response code="200">List of matching vehicles (may be empty).</response>
    [HttpGet("por-tipo")]
    [ProducesResponseType(typeof(IEnumerable<PrqAutomovil>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PrqAutomovil>>> GetByPartialTipo(
        [FromQuery] string tipo,
        CancellationToken cancellationToken)
    {
        var results = await _repository.GetByPartialTipoAsync(tipo, cancellationToken);
        return Ok(results);
    }
}
