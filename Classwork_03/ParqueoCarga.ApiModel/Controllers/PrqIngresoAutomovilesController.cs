using Microsoft.AspNetCore.Mvc;
using ParqueoCarga.DbModel.Models;
using ParqueolCarga.RepositoryModel.Dtos;
using ParqueolCarga.RepositoryModel.Interfaces;

namespace ParqueoCarga.ApiModel.Controllers;

/// <summary>
/// Exposes CRUD and query endpoints for ingresos de automoviles.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class PrqIngresoAutomovilesController : ControllerBase
{
    private readonly IPrqIngresoAutomovilRepository _repository;

    public PrqIngresoAutomovilesController(IPrqIngresoAutomovilRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Retrieves all ingresos de automoviles.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<PrqIngresoAutomovil>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Retrieves an ingreso automovil by consecutivo.
    /// </summary>
    [HttpGet("{consecutivo:uint}")]
    public async Task<ActionResult<PrqIngresoAutomovil>> GetById(uint consecutivo, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(consecutivo, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    /// <summary>
    /// Creates an ingreso automovil.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PrqIngresoAutomovil>> Create(
        PrqIngresoAutomovil entity,
        CancellationToken cancellationToken)
    {
        var created = await _repository.CreateAsync(entity, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { consecutivo = created.Consecutivo }, created);
    }

    /// <summary>
    /// Updates an ingreso automovil by consecutivo.
    /// </summary>
    [HttpPut("{consecutivo:uint}")]
    public async Task<IActionResult> Update(
        uint consecutivo,
        PrqIngresoAutomovil entity,
        CancellationToken cancellationToken)
    {
        if (consecutivo != entity.Consecutivo)
        {
            return BadRequest("Route consecutivo must match body consecutivo.");
        }

        var updated = await _repository.UpdateAsync(entity, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Deletes an ingreso automovil by consecutivo.
    /// </summary>
    [HttpDelete("{consecutivo:uint}")]
    public async Task<IActionResult> Delete(uint consecutivo, CancellationToken cancellationToken)
    {
        var deleted = await _repository.DeleteAsync(consecutivo, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Obtains the hourly price for a given parqueo id.
    /// </summary>
    [HttpGet("parqueo/{idParqueo:uint}/precio-hora")]
    public async Task<ActionResult<decimal?>> ObtenerPrecioPorHoraPorParqueo(uint idParqueo, CancellationToken cancellationToken)
    {
        var precio = await _repository.ObtenerPrecioPorHoraPorParqueo(idParqueo, cancellationToken);
        if (precio is null)
        {
            return NotFound();
        }

        return Ok(precio);
    }

    /// <summary>
    /// Searches ingresos by automovil tipo in a date range.
    /// </summary>
    [HttpGet("search/tipo-date-range")]
    public async Task<ActionResult<List<PrqIngresoAutomovilQueryResult>>> GetByTipoAndDateRange(
        [FromQuery] string tipo,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken)
    {
        var items = await _repository.GetByTipoAndDateRange(tipo, startDate, endDate, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Searches ingresos by parqueo provincia in a date range.
    /// </summary>
    [HttpGet("search/provincia-date-range")]
    public async Task<ActionResult<List<PrqIngresoAutomovilQueryResult>>> GetByProvinciaAndDateRange(
        [FromQuery] string provincia,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken)
    {
        var items = await _repository.GetByProvinciaAndDateRange(provincia, startDate, endDate, cancellationToken);
        return Ok(items);
    }
}
