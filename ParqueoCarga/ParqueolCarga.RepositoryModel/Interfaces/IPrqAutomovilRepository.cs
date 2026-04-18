using ParqueoCarga.DbModel.Models;

namespace ParqueolCarga.RepositoryModel.Interfaces;

public interface IPrqAutomovilRepository
{
    Task<List<PrqAutomovil>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<PrqAutomovil?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);

    Task<PrqAutomovil> CreateAsync(PrqAutomovil entity, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(PrqAutomovil entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default);

    Task<List<PrqAutomovil>> GetByPartialColorAsync(string color, CancellationToken cancellationToken = default);

    Task<List<PrqAutomovil>> GetByYearRangeAsync(int startYear, int endYear, CancellationToken cancellationToken = default);

    Task<List<PrqAutomovil>> GetByPartialFabricanteAsync(string fabricante, CancellationToken cancellationToken = default);

    Task<List<PrqAutomovil>> GetByPartialTipoAsync(string tipo, CancellationToken cancellationToken = default);
}