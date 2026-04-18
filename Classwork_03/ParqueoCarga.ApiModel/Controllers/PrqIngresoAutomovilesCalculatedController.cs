using Microsoft.AspNetCore.Mvc;
using ParqueoCarga.DbModel.Models;
using ParqueolCarga.RepositoryModel.Interfaces;

namespace ParqueoCarga.ApiModel.Controllers;

/// <summary>
/// Exposes CRUD endpoints for ingreso automoviles with calculated fields.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class PrqIngresoAutomovilesCalculatedController : ControllerBase
{
    private readonly IPrqIngresoAutomovilRepository _repository;

    public PrqIngresoAutomovilesCalculatedController(IPrqIngresoAutomovilRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Retrieves all ingreso automoviles including calculated fields.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<PrqIngresoAutomovil>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Retrieves an ingreso automovil by consecutivo including calculated fields.
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
    /// Creates an ingreso automovil record.
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
}
