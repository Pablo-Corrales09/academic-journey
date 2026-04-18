using ParqueoCarga.DbModel.Models;

namespace ParqueolCarga.RepositoryModel.Interfaces;

public interface IPrqParqueoRepository
{
    Task<List<PrqParqueo>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<PrqParqueo?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);

    Task<PrqParqueo> CreateAsync(PrqParqueo entity, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(PrqParqueo entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default);

    Task<List<PrqParqueo>> GetByFiltrosAsync(
        string? provincia,
        string? nombre,
        decimal minPrice,
        decimal maxPrice,
        CancellationToken cancellationToken = default);
}