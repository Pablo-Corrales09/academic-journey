using Microsoft.EntityFrameworkCore;
using ParqueoCarga.DbModel.Data;
using ParqueoCarga.DbModel.Models;
using ParqueolCarga.RepositoryModel.Interfaces;

namespace ParqueolCarga.RepositoryModel.Repositories;

public sealed class PrqParqueoRepository : IPrqParqueoRepository
{
    private readonly ParqueoCargaContext _dbContext;

    public PrqParqueoRepository(ParqueoCargaContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<PrqParqueo>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.PrqParqueos
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<PrqParqueo?> GetByIdAsync(uint id, CancellationToken cancellationToken = default)
    {
        return _dbContext.PrqParqueos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<PrqParqueo> CreateAsync(PrqParqueo entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.PrqParqueos.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> UpdateAsync(PrqParqueo entity, CancellationToken cancellationToken = default)
    {
        var existingEntity = await _dbContext.PrqParqueos
            .FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken);

        if (existingEntity is null)
        {
            return false;
        }

        _dbContext.Entry(existingEntity).CurrentValues.SetValues(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.PrqParqueos
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        _dbContext.PrqParqueos.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<List<PrqParqueo>> GetByFiltrosAsync(
        string? provincia,
        string? nombre,
        decimal minPrice,
        decimal maxPrice,
        CancellationToken cancellationToken = default)
    {
        if (minPrice > maxPrice)
        {
            (minPrice, maxPrice) = (maxPrice, minPrice);
        }

        IQueryable<PrqParqueo> query = _dbContext.PrqParqueos
            .AsNoTracking()
            .Where(x => x.PrecioHora >= minPrice && x.PrecioHora <= maxPrice);

        if (!string.IsNullOrWhiteSpace(provincia))
        {
            var provinciaPattern = $"%{provincia.Trim()}%";
            query = query.Where(x => EF.Functions.Like(x.Provincia, provinciaPattern));
        }

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            var nombrePattern = $"%{nombre.Trim()}%";
            query = query.Where(x => EF.Functions.Like(x.Nombre, nombrePattern));
        }

        return query.ToListAsync(cancellationToken);
    }
}