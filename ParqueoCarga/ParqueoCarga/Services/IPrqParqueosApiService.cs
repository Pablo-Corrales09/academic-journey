using ParqueoCarga.ViewModels.Parqueos;

namespace ParqueoCarga.Services;

public interface IPrqParqueosApiService
{
    Task<IReadOnlyList<ParqueoViewModel>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ParqueoViewModel?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);

    Task<ParqueoViewModel> CreateAsync(ParqueoUpsertViewModel request, CancellationToken cancellationToken = default);

    Task UpdateAsync(uint id, ParqueoUpsertViewModel request, CancellationToken cancellationToken = default);

    Task DeleteAsync(uint id, CancellationToken cancellationToken = default);
}
