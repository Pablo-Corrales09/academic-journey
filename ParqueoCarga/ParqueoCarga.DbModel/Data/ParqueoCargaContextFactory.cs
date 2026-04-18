using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using ParqueoCarga.DbModel.Configuration;

namespace ParqueoCarga.DbModel.Data;

public sealed class ParqueoCargaContextFactory : IDesignTimeDbContextFactory<ParqueoCargaContext>
{
    public ParqueoCargaContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<ParqueoCargaContextFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("MyConnectionString")
            ?? ConnectionStringFactory.TryCreateFromConfiguration(configuration)
            ?? throw new InvalidOperationException(
                "No database configuration was found. Set user secrets for ConnectionStrings:MyConnectionString or the DatabaseSettings values.");

        var serverVersionText = configuration["DatabaseSettings:ServerVersion"];
        var serverVersion = string.IsNullOrWhiteSpace(serverVersionText)
            ? ServerVersion.AutoDetect(connectionString)
            : ServerVersion.Parse(serverVersionText);

        var optionsBuilder = new DbContextOptionsBuilder<ParqueoCargaContext>();
        optionsBuilder.UseMySql(connectionString, serverVersion);

        return new ParqueoCargaContext(optionsBuilder.Options);
    }
}