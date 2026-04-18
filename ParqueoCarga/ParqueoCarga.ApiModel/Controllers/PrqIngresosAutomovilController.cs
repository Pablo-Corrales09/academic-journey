using Microsoft.AspNetCore.Mvc;
using ParqueoCarga.ApiModel.Dtos;
using ParqueoCarga.DbModel.Models;
using ParqueolCarga.RepositoryModel.Dtos;
using ParqueolCarga.RepositoryModel.Interfaces;

namespace ParqueoCarga.ApiModel.Controllers;

/// <summary>
/// Manages vehicle entry records (<c>PrqIngresoAutomovil</c>).
/// Each record tracks when a vehicle entered and exited a parking lot,
/// and exposes calculated fields such as stay duration and total cost.
/// </summary>
[ApiController]
[Route("api/prq-ingresos-automovil")]
[Produces("application/json")]
public sealed class PrqIngresosAutomovilController : ControllerBase
{
    private readonly IPrqIngresoAutomovilRepository _repository;

    /// <summary>Initialises the controller with the required repository.</summary>
    public PrqIngresosAutomovilController(IPrqIngresoAutomovilRepository repository)
    {
        _repository = repository;
    }

    // -------------------------------------------------------------------------
    // CRUD
    // -------------------------------------------------------------------------

    /// <summary>Returns all vehicle entry records.</summary>
    /// <remarks>
    /// Each returned record includes calculated fields:
    /// <c>duracionEstadiaMinutos</c>, <c>duracionEstadiaHoras</c>, and <c>montoTotalPagar</c>.
    /// These are <c>null</c> when the vehicle has not yet exited.
    /// </remarks>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <response code="200">List of entry records (may be empty).</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PrqIngresoAutomovil>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PrqIngresoAutomovil>>> GetAll(
        CancellationToken cancellationToken)
    {
        var ingresos = await _repository.GetAllAsync(cancellationToken);
        return Ok(ingresos);
    }

    /// <summary>Returns a single entry record by its consecutive number.</summary>
    /// <param name="consecutivo">Primary key of the entry record.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <response code="200">The requested entry record.</response>
    /// <response code="404">Entry record not found.</response>
    [HttpGet("{consecutivo:long}")]
    [ProducesResponseType(typeof(PrqIngresoAutomovil), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrqIngresoAutomovil>> GetById(
        [FromRoute] uint consecutivo,
        CancellationToken cancellationToken)
    {
        var ingreso = await _repository.GetByIdAsync(consecutivo, cancellationToken);
        return ingreso is null ? NotFound() : Ok(ingreso);
    }

    /// <summary>Creates a new vehicle entry record.</summary>
    /// <param name="request">Entry record data.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <response code="201">The newly created entry record.</response>
    /// <response code="400">Validation errors in the request body.</response>
    [HttpPost]
    [ProducesResponseType(typeof(PrqIngresoAutomovil), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PrqIngresoAutomovil>> Create(
        [FromBody] PrqIngresoAutomovilRequest request,
        CancellationToken cancellationToken)
    {
        var entity = new PrqIngresoAutomovil
        {
            IdParqueo = request.IdParqueo,
            IdAutomovil = request.IdAutomovil,
            FechaEntrada = request.FechaEntrada,
            FechaSalida = request.FechaSalida
        };

        var created = await _repository.CreateAsync(entity, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { consecutivo = created.Consecutivo }, created);
    }

    /// <summary>Updates an existing entry record.</summary>
    /// <param name="consecutivo">Primary key of the entry record to update.</param>
    /// <param name="request">Updated entry record data.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <response code="204">Update succeeded.</response>
    /// <response code="400">Validation errors in the request body.</response>
    /// <response code="404">Entry record not found.</response>
    [HttpPut("{consecutivo:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] uint consecutivo,
        [FromBody] PrqIngresoAutomovilRequest request,
        CancellationToken cancellationToken)
    {
        var entity = new PrqIngresoAutomovil
        {
            Consecutivo = consecutivo,
            IdParqueo = request.IdParqueo,
            IdAutomovil = request.IdAutomovil,
            FechaEntrada = request.FechaEntrada,
            FechaSalida = request.FechaSalida
        };

        var updated = await _repository.UpdateAsync(entity, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Deletes an entry record by its consecutive number.</summary>
    /// <param name="consecutivo">Primary key of the entry record to delete.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <response code="204">Deletion succeeded.</response>
    /// <response code="404">Entry record not found.</response>
    [HttpDelete("{consecutivo:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] uint consecutivo,
        CancellationToken cancellationToken)
    {
        var deleted = await _repository.DeleteAsync(consecutivo, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    // -------------------------------------------------------------------------
    // Custom queries
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the hourly rate (<c>PrecioHora</c>) of the parking lot associated with the given ID.
    /// </summary>
    /// <param name="idParqueo">ID of the parking lot.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <response code="200">The hourly rate as a decimal number.</response>
    /// <response code="404">No parking lot with the given ID was found.</response>
    [HttpGet("precio-por-hora/{idParqueo:long}")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<decimal>> ObtenerPrecioPorHora(
        [FromRoute] uint idParqueo,
        CancellationToken cancellationToken)
    {
        var precio = await _repository.ObtenerPrecioPorHoraPorParqueo(idParqueo, cancellationToken);
        return precio is null ? NotFound() : Ok(precio);
    }

    /// <summary>
    /// Returns entry records filtered by vehicle type and entry date range.
    /// </summary>
    /// <param name="tipo">Vehicle type (e.g. "Sedán"). Partial match supported.</param>
    /// <param name="startDate">Start of the entry date range (inclusive).</param>
    /// <param name="endDate">End of the entry date range (inclusive).</param>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <response code="200">List of matching entry records (may be empty).</response>
    /// <response code="400"><paramref name="startDate"/> is after <paramref name="endDate"/>.</response>
    [HttpGet("por-tipo-y-fecha")]
    [ProducesResponseType(typeof(IEnumerable<PrqIngresoAutomovilQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<PrqIngresoAutomovilQueryResult>>> GetByTipoAndDateRange(
        [FromQuery] string tipo,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken)
    {
        if (startDate > endDate)
            return BadRequest("startDate must be before or equal to endDate.");

        var results = await _repository.GetByTipoAndDateRange(tipo, startDate, endDate, cancellationToken);
        return Ok(results);
    }

    /// <summary>
    /// Returns entry records filtered by parking lot province and entry date range.
    /// </summary>
    /// <param name="provincia">Province of the parking lot (e.g. "San José"). Partial match supported.</param>
    /// <param name="startDate">Start of the entry date range (inclusive).</param>
    /// <param name="endDate">End of the entry date range (inclusive).</param>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <response code="200">List of matching entry records (may be empty).</response>
    /// <response code="400"><paramref name="startDate"/> is after <paramref name="endDate"/>.</response>
    [HttpGet("por-provincia-y-fecha")]
    [ProducesResponseType(typeof(IEnumerable<PrqIngresoAutomovilQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<PrqIngresoAutomovilQueryResult>>> GetByProvinciaAndDateRange(
        [FromQuery] string provincia,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken)
    {
        if (startDate > endDate)
            return BadRequest("startDate must be before or equal to endDate.");

        var results = await _repository.GetByProvinciaAndDateRange(provincia, startDate, endDate, cancellationToken);
        return Ok(results);
    }
}
