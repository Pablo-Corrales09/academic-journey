using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using ParqueoCarga.DbModel.Extensions;
using ParqueolCarga.RepositoryModel.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ----- Services -----
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Skip real database setup when running integration tests to prevent the
// MySQL and InMemory providers conflicting in the same service container.
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddParqueoCargaDatabase(builder.Configuration);
}

builder.Services.AddParqueolCargaRepositories();

// ----- Swagger / OpenAPI -----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ParqueoCarga API",
        Version = "v1",
        Description = "RESTful API for managing parking lots (parqueos), vehicles (automóviles), " +
                      "and vehicle entry records (ingresos de automóviles)."
    });

    // Include XML doc comments in Swagger UI
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// ----- Middleware -----
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ParqueoCarga API v1");
    c.RoutePrefix = string.Empty; // Swagger UI at root "/"
});

app.UseAuthorization();
app.MapControllers();

app.Run();

/// <summary>
/// Entry-point partial class required for WebApplicationFactory in integration tests.
/// </summary>
public partial class Program { }
