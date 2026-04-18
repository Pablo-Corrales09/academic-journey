using ParqueoCarga.ViewModels.IngresosAutomovil;

namespace ParqueoCarga.Services;

public interface IPrqIngresosAutomovilApiService
{
    Task<IReadOnlyList<IngresoAutomovilViewModel>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IngresoAutomovilViewModel?> GetByIdAsync(uint consecutivo, CancellationToken cancellationToken = default);

    Task<IngresoAutomovilViewModel> CreateAsync(IngresoAutomovilUpsertViewModel request, CancellationToken cancellationToken = default);

    Task UpdateAsync(uint consecutivo, IngresoAutomovilUpsertViewModel request, CancellationToken cancellationToken = default);

    Task DeleteAsync(uint consecutivo, CancellationToken cancellationToken = default);

    Task<decimal?> ObtenerPrecioPorHoraAsync(uint idParqueo, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IngresoAutomovilViewModel>> GetByTipoAndDateRangeAsync(string tipo, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IngresoAutomovilViewModel>> GetByProvinciaAndDateRangeAsync(string provincia, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
