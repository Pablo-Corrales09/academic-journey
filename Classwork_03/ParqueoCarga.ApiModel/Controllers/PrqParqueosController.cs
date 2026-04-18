using Microsoft.AspNetCore.Mvc;
using ParqueoCarga.DbModel.Models;
using ParqueolCarga.RepositoryModel.Interfaces;

namespace ParqueoCarga.ApiModel.Controllers;

/// <summary>
/// Exposes CRUD and filter endpoints for parqueos.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class PrqParqueosController : ControllerBase
{
    private readonly IPrqParqueoRepository _repository;

    public PrqParqueosController(IPrqParqueoRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Retrieves all parqueos.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<PrqParqueo>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Retrieves a parqueo by id.
    /// </summary>
    [HttpGet("{id:uint}")]
    public async Task<ActionResult<PrqParqueo>> GetById(uint id, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    /// <summary>
    /// Creates a parqueo.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PrqParqueo>> Create(PrqParqueo entity, CancellationToken cancellationToken)
    {
        var created = await _repository.CreateAsync(entity, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates a parqueo by id.
    /// </summary>
    [HttpPut("{id:uint}")]
    public async Task<IActionResult> Update(uint id, PrqParqueo entity, CancellationToken cancellationToken)
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
    /// Deletes a parqueo by id.
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
    /// Searches parqueos by optional provincia, nombre and a price range.
    /// </summary>
    [HttpGet("search/filtros")]
    public async Task<ActionResult<List<PrqParqueo>>> GetByFiltros(
        [FromQuery] string? provincia,
        [FromQuery] string? nombre,
        [FromQuery] decimal minPrice = 0,
        [FromQuery] decimal maxPrice = decimal.MaxValue,
        CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetByFiltrosAsync(provincia, nombre, minPrice, maxPrice, cancellationToken);
        return Ok(items);
    }
}
