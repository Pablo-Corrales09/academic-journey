using ParqueoCarga.DbModel.Models;

namespace ParqueolCarga.RepositoryModel.Interfaces;

public interface IPrqParqueoRepository
{
    Task<PrqParqueo?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);

    Task<List<PrqParqueo>> GetByFiltrosAsync(
        string? provincia,
        string? nombre,
        decimal minPrice,
        decimal maxPrice,
        CancellationToken cancellationToken = default);
}