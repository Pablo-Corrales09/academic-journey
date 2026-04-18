using ParqueoCarga.DbModel.Models;
using ParqueolCarga.RepositoryModel.Dtos;

namespace ParqueolCarga.RepositoryModel.Interfaces;

public interface IPrqIngresoAutomovilRepository
{
    Task<List<PrqIngresoAutomovil>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<PrqIngresoAutomovil?> GetByIdAsync(uint consecutivo, CancellationToken cancellationToken = default);

    Task<PrqIngresoAutomovil> CreateAsync(PrqIngresoAutomovil entity, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(PrqIngresoAutomovil entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(uint consecutivo, CancellationToken cancellationToken = default);

    Task<decimal?> ObtenerPrecioPorHoraPorParqueo(uint idParqueo, CancellationToken cancellationToken = default);

    Task<List<PrqIngresoAutomovilQueryResult>> GetByTipoAndDateRange(
        string tipo,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    Task<List<PrqIngresoAutomovilQueryResult>> GetByProvinciaAndDateRange(
        string provincia,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);
}