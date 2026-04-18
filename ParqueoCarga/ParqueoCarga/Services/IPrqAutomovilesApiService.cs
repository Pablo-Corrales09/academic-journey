using ParqueoCarga.ViewModels.Automoviles;

namespace ParqueoCarga.Services;

public interface IPrqAutomovilesApiService
{
    Task<IReadOnlyList<AutomovilViewModel>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<AutomovilViewModel?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);

    Task<AutomovilViewModel> CreateAsync(AutomovilUpsertViewModel request, CancellationToken cancellationToken = default);

    Task UpdateAsync(uint id, AutomovilUpsertViewModel request, CancellationToken cancellationToken = default);

    Task DeleteAsync(uint id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AutomovilViewModel>> GetByColorAsync(string color, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AutomovilViewModel>> GetByYearRangeAsync(short startYear, short endYear, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AutomovilViewModel>> GetByFabricanteAsync(string fabricante, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AutomovilViewModel>> GetByTipoAsync(string tipo, CancellationToken cancellationToken = default);
}