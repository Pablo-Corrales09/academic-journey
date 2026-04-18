using Microsoft.EntityFrameworkCore;
using ParqueoCarga.DbModel.Data;
using ParqueoCarga.DbModel.Models;
using ParqueolCarga.RepositoryModel.Interfaces;

namespace ParqueolCarga.RepositoryModel.Repositories;

public sealed class PrqAutomovilRepository : IPrqAutomovilRepository
{
    private readonly ParqueoCargaContext _dbContext;

    public PrqAutomovilRepository(ParqueoCargaContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<PrqAutomovil?> GetByIdAsync(uint id, CancellationToken cancellationToken = default)
    {
        return _dbContext.PrqAutomoviles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<List<PrqAutomovil>> GetByPartialColorAsync(string color, CancellationToken cancellationToken = default)
    {
        IQueryable<PrqAutomovil> query = _dbContext.PrqAutomoviles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(color))
        {
            var pattern = $"%{color.Trim()}%";
            query = query.Where(x => EF.Functions.Like(x.Color, pattern));
        }

        return query.ToListAsync(cancellationToken);
    }

    public Task<List<PrqAutomovil>> GetByYearRangeAsync(int startYear, int endYear, CancellationToken cancellationToken = default)
    {
        if (startYear > endYear)
        {
            (startYear, endYear) = (endYear, startYear);
        }

        return _dbContext.PrqAutomoviles
            .AsNoTracking()
            .Where(x => x.Anio >= startYear && x.Anio <= endYear)
            .ToListAsync(cancellationToken);
    }

    public Task<List<PrqAutomovil>> GetByPartialFabricanteAsync(string fabricante, CancellationToken cancellationToken = default)
    {
        IQueryable<PrqAutomovil> query = _dbContext.PrqAutomoviles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(fabricante))
        {
            var pattern = $"%{fabricante.Trim()}%";
            query = query.Where(x => EF.Functions.Like(x.Fabricante, pattern));
        }

        return query.ToListAsync(cancellationToken);
    }

    public Task<List<PrqAutomovil>> GetByPartialTipoAsync(string tipo, CancellationToken cancellationToken = default)
    {
        IQueryable<PrqAutomovil> query = _dbContext.PrqAutomoviles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(tipo))
        {
            var pattern = $"%{tipo.Trim()}%";
            query = query.Where(x => EF.Functions.Like(x.Tipo, pattern));
        }

        return query.ToListAsync(cancellationToken);
    }
}