using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ParqueoCarga.DbModel.Data;

namespace ParqueoCarga.ApiModel.Tests.TestUtilities;

/// <summary>
/// Custom <see cref="WebApplicationFactory{TProgram}"/> that replaces the MySQL
/// <see cref="ParqueoCargaContext"/> with an isolated EF Core in-memory database,
/// so integration tests run without a live database.
/// </summary>
public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set environment so appsettings.Testing.json is loaded and so that
        // Program.cs skips the real MySQL registration entirely.
        builder.UseEnvironment("Testing");

        // Safety-net config in case appsettings.Testing.json is not in output dir.
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:MyConnectionString"] =
                    "Server=localhost;Port=3306;Database=test;User ID=test;Password=test;SslMode=None;",
                ["DatabaseSettings:ServerVersion"] = "8.0.0-mysql"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Generate the DB name ONCE so all scopes within this factory instance
            // (seed scope + HTTP request scopes) share the same in-memory database.
            var dbName = Guid.NewGuid().ToString("N");
            services.AddDbContext<ParqueoCargaContext>(options =>
                options.UseInMemoryDatabase(dbName));
        });
    }

    /// <summary>
    /// Creates a scoped <see cref="ParqueoCargaContext"/> for seeding or asserting
    /// database state inside a test. The caller is responsible for disposing the scope.
    /// </summary>
    public (IServiceScope Scope, ParqueoCargaContext Context) CreateDbContext()
    {
        var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ParqueoCargaContext>();
        return (scope, context);
    }
}
