using ParqueoCarga.DbModel.Models;
using ParqueolCarga.RepositoryModel.Dtos;

namespace ParqueolCarga.RepositoryModel.Interfaces;

public interface IPrqIngresoAutomovilRepository
{
    Task<PrqIngresoAutomovil?> GetByIdAsync(uint consecutivo, CancellationToken cancellationToken = default);

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