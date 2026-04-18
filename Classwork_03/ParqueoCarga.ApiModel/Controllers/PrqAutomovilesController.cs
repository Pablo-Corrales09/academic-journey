using Microsoft.AspNetCore.Mvc;
using ParqueoCarga.DbModel.Models;
using ParqueolCarga.RepositoryModel.Interfaces;

namespace ParqueoCarga.ApiModel.Controllers;

/// <summary>
/// Exposes CRUD and search endpoints for automoviles.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class PrqAutomovilesController : ControllerBase
{
    private readonly IPrqAutomovilRepository _repository;

    public PrqAutomovilesController(IPrqAutomovilRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Retrieves all automoviles.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<PrqAutomovil>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Retrieves a single automovil by id.
    /// </summary>
    [HttpGet("{id:uint}")]
    public async Task<ActionResult<PrqAutomovil>> GetById(uint id, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    /// <summary>
    /// Creates a new automovil.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PrqAutomovil>> Create(PrqAutomovil entity, CancellationToken cancellationToken)
    {
        var created = await _repository.CreateAsync(entity, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an existing automovil.
    /// </summary>
    [HttpPut("{id:uint}")]
    public async Task<IActionResult> Update(uint id, PrqAutomovil entity, CancellationToken cancellationToken)
    {
        if (id != entity.Id)
        {
            return BadRequest("Route id must match body id.");
        }

        var updated = await _repository.UpdateAsync(entity, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Deletes an automovil by id.
    /// </summary>
    [HttpDelete("{id:uint}")]
    public async Task<IActionResult> Delete(uint id, CancellationToken cancellationToken)
    {
        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Searches automoviles by partial color.
    /// </summary>
    [HttpGet("search/color")]
    public async Task<ActionResult<List<PrqAutomovil>>> GetByPartialColor(
        [FromQuery] string color,
        CancellationToken cancellationToken)
    {
        var items = await _repository.GetByPartialColorAsync(color, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Searches automoviles by a year range.
    /// </summary>
    [HttpGet("search/year-range")]
    public async Task<ActionResult<List<PrqAutomovil>>> GetByYearRange(
        [FromQuery] int startYear,
        [FromQuery] int endYear,
        CancellationToken cancellationToken)
    {
        var items = await _repository.GetByYearRangeAsync(startYear, endYear, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Searches automoviles by partial fabricante.
    /// </summary>
    [HttpGet("search/fabricante")]
    public async Task<ActionResult<List<PrqAutomovil>>> GetByPartialFabricante(
        [FromQuery] string fabricante,
        CancellationToken cancellationToken)
    {
        var items = await _repository.GetByPartialFabricanteAsync(fabricante, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Searches automoviles by partial tipo.
    /// </summary>
    [HttpGet("search/tipo")]
    public async Task<ActionResult<List<PrqAutomovil>>> GetByPartialTipo(
        [FromQuery] string tipo,
        CancellationToken cancellationToken)
    {
        var items = await _repository.GetByPartialTipoAsync(tipo, cancellationToken);
        return Ok(items);
    }
}
