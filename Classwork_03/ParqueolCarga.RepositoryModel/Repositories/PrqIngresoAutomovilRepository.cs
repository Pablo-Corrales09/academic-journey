using Microsoft.EntityFrameworkCore;
using ParqueoCarga.DbModel.Data;
using ParqueoCarga.DbModel.Models;
using ParqueolCarga.RepositoryModel.Dtos;
using ParqueolCarga.RepositoryModel.Interfaces;

namespace ParqueolCarga.RepositoryModel.Repositories;

public sealed class PrqIngresoAutomovilRepository : IPrqIngresoAutomovilRepository
{
    private readonly ParqueoCargaContext _dbContext;

    public PrqIngresoAutomovilRepository(ParqueoCargaContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<PrqIngresoAutomovil>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.PrqIngresoAutomoviles
            .AsNoTracking()
            .Include(x => x.IdAutomovilNavigation)
            .Include(x => x.IdParqueoNavigation)
            .ToListAsync(cancellationToken);
    }

    public Task<PrqIngresoAutomovil?> GetByIdAsync(uint consecutivo, CancellationToken cancellationToken = default)
    {
        return _dbContext.PrqIngresoAutomoviles
            .AsNoTracking()
            .Include(x => x.IdAutomovilNavigation)
            .Include(x => x.IdParqueoNavigation)
            .FirstOrDefaultAsync(x => x.Consecutivo == consecutivo, cancellationToken);
    }

    public async Task<PrqIngresoAutomovil> CreateAsync(PrqIngresoAutomovil entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.PrqIngresoAutomoviles.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> UpdateAsync(PrqIngresoAutomovil entity, CancellationToken cancellationToken = default)
    {
        var existingEntity = await _dbContext.PrqIngresoAutomoviles
            .FirstOrDefaultAsync(x => x.Consecutivo == entity.Consecutivo, cancellationToken);

        if (existingEntity is null)
        {
            return false;
        }

        _dbContext.Entry(existingEntity).CurrentValues.SetValues(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(uint consecutivo, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.PrqIngresoAutomoviles
            .FirstOrDefaultAsync(x => x.Consecutivo == consecutivo, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        _dbContext.PrqIngresoAutomoviles.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<decimal?> ObtenerPrecioPorHoraPorParqueo(uint idParqueo, CancellationToken cancellationToken = default)
    {
        return _dbContext.PrqParqueos
            .AsNoTracking()
            .Where(x => x.Id == idParqueo)
            .Select(x => (decimal?)x.PrecioHora)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<PrqIngresoAutomovilQueryResult>> GetByTipoAndDateRange(
        string tipo,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var entradas = await _dbContext.PrqIngresoAutomoviles
            .AsNoTracking()
            .Include(x => x.IdAutomovilNavigation)
            .Where(x =>
                EF.Functions.Like(x.IdAutomovilNavigation.Tipo, $"%{tipo.Trim()}%") &&
                x.FechaEntrada >= startDate &&
                x.FechaEntrada <= endDate &&
                (x.FechaSalida == null || (x.FechaSalida >= startDate && x.FechaSalida <= endDate)))
            .ToListAsync(cancellationToken);

        return await BuildQueryResultsAsync(entradas, cancellationToken);
    }

    public async Task<List<PrqIngresoAutomovilQueryResult>> GetByProvinciaAndDateRange(
        string provincia,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var entradas = await _dbContext.PrqIngresoAutomoviles
            .AsNoTracking()
            .Include(x => x.IdParqueoNavigation)
            .Where(x =>
                EF.Functions.Like(x.IdParqueoNavigation.Provincia, $"%{provincia.Trim()}%") &&
                x.FechaEntrada >= startDate &&
                x.FechaEntrada <= endDate &&
                (x.FechaSalida == null || (x.FechaSalida >= startDate && x.FechaSalida <= endDate)))
            .ToListAsync(cancellationToken);

        return await BuildQueryResultsAsync(entradas, cancellationToken);
    }

    private async Task<List<PrqIngresoAutomovilQueryResult>> BuildQueryResultsAsync(
        List<PrqIngresoAutomovil> entradas,
        CancellationToken cancellationToken)
    {
        var precioPorParqueo = new Dictionary<uint, decimal?>();
        var results = new List<PrqIngresoAutomovilQueryResult>(entradas.Count);

        foreach (var entrada in entradas)
        {
            if (!precioPorParqueo.TryGetValue(entrada.IdParqueo, out var precioHora))
            {
                precioHora = await ObtenerPrecioPorHoraPorParqueo(entrada.IdParqueo, cancellationToken);
                precioPorParqueo[entrada.IdParqueo] = precioHora;
            }

            decimal? total = null;
            if (entrada.FechaSalida is not null && precioHora is not null)
            {
                var horas = (decimal)(entrada.FechaSalida.Value - entrada.FechaEntrada).TotalHours;
                total = decimal.Round(horas * precioHora.Value, 2, MidpointRounding.AwayFromZero);
            }

            results.Add(new PrqIngresoAutomovilQueryResult
            {
                Consecutivo = entrada.Consecutivo,
                IdParqueo = entrada.IdParqueo,
                IdAutomovil = entrada.IdAutomovil,
                FechaEntrada = entrada.FechaEntrada,
                FechaSalida = entrada.FechaSalida,
                MontoTotalPagar = total
            });
        }

        return results;
    }
}