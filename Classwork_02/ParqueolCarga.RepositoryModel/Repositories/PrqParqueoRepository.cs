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

    public Task<PrqParqueo?> GetByIdAsync(uint id, CancellationToken cancellationToken = default)
    {
        return _dbContext.PrqParqueos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
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