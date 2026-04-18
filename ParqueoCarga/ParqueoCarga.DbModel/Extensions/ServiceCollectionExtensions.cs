using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ParqueoCarga.DbModel.Configuration;
using ParqueoCarga.DbModel.Data;

namespace ParqueoCarga.DbModel.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddParqueoCargaDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MyConnectionString")
            ?? ConnectionStringFactory.TryCreateFromConfiguration(configuration)
            ?? throw new InvalidOperationException(
                "No database configuration was found. Set user secrets for ConnectionStrings:MyConnectionString or the DatabaseSettings values.");

        var serverVersionText = configuration["DatabaseSettings:ServerVersion"];
        var serverVersion = string.IsNullOrWhiteSpace(serverVersionText)
            ? ServerVersion.AutoDetect(connectionString)
            : ServerVersion.Parse(serverVersionText);

        services.AddDbContext<ParqueoCargaContext>(options =>
            options.UseMySql(connectionString, serverVersion));

        return services;
    }
}