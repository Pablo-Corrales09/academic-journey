using Microsoft.Extensions.DependencyInjection;
using ParqueolCarga.RepositoryModel.Interfaces;
using ParqueolCarga.RepositoryModel.Repositories;

namespace ParqueolCarga.RepositoryModel.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddParqueolCargaRepositories(this IServiceCollection services)
    {
        services.AddScoped<IPrqAutomovilRepository, PrqAutomovilRepository>();
        services.AddScoped<IPrqParqueoRepository, PrqParqueoRepository>();
        services.AddScoped<IPrqIngresoAutomovilRepository, PrqIngresoAutomovilRepository>();
        return services;
    }
}