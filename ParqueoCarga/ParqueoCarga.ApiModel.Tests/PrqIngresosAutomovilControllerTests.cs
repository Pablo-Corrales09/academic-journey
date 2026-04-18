using System.Net;
using System.Net.Http.Json;
using ParqueoCarga.ApiModel.Dtos;
using ParqueoCarga.ApiModel.Tests.TestUtilities;
using ParqueoCarga.DbModel.Models;
using ParqueolCarga.RepositoryModel.Dtos;

namespace ParqueoCarga.ApiModel.Tests;

public class PrqIngresosAutomovilControllerTests
{
    // Helper: seeds a PrqParqueo and PrqAutomovil required by FK constraints.
    private static async Task SeedPrerequisitesAsync(
        ParqueoCarga.DbModel.Data.ParqueoCargaContext context,
        uint parqueoId = 1,
        uint automovilId = 1)
    {
        if (!context.PrqParqueos.Any(p => p.Id == parqueoId))
        {
            context.PrqParqueos.Add(new PrqParqueo
            {
                Id = parqueoId,
                Provincia = "San José",
                Nombre = "Parqueo Test",
                PrecioHora = 500m
            });
        }

        if (!context.PrqAutomoviles.Any(a => a.Id == automovilId))
        {
            context.PrqAutomoviles.Add(new PrqAutomovil
            {
                Id = automovilId,
                Color = "Rojo",
                Anio = 2020,
                Fabricante = "Toyota",
                Tipo = "Sedán"
            });
        }

        await context.SaveChangesAsync();
    }

    // -------------------------------------------------------------------------
    // GET /api/prq-ingresos-automovil
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAll_Returns_200_With_EmptyList_WhenNoneExist()
    {
        await using var factory = new ApiWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/prq-ingresos-automovil");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<PrqIngresoAutomovil>>();
        Assert.NotNull(body);
        Assert.Empty(body);
    }

    [Fact]
    public async Task GetAll_Returns_200_With_SeededIngresos()
    {
        await using var factory = new ApiWebApplicationFactory();
        var (scope, context) = factory.CreateDbContext();
        using (scope)
        {
            await SeedPrerequisitesAsync(context);
            context.PrqIngresoAutomoviles.Add(new PrqIngresoAutomovil
            {
                Consecutivo = 1,
                IdParqueo = 1,
                IdAutomovil = 1,
                FechaEntrada = new DateTime(2024, 1, 10, 8, 0, 0, DateTimeKind.Utc)
            });
            await context.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/prq-ingresos-automovil");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<PrqIngresoAutomovil>>();
        Assert.NotNull(body);
        Assert.Single(body);
    }

    // -------------------------------------------------------------------------
    // GET /api/prq-ingresos-automovil/{consecutivo}
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetById_Returns_200_WhenExists()
    {
        await using var factory = new ApiWebApplicationFactory();
        var (scope, context) = factory.CreateDbContext();
        using (scope)
        {
            await SeedPrerequisitesAsync(context);
            context.PrqIngresoAutomoviles.Add(new PrqIngresoAutomovil
            {
                Consecutivo = 10,
                IdParqueo = 1,
                IdAutomovil = 1,
                FechaEntrada = new DateTime(2024, 2, 5, 9, 0, 0, DateTimeKind.Utc),
                FechaSalida = new DateTime(2024, 2, 5, 11, 0, 0, DateTimeKind.Utc)
            });
            await context.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/prq-ingresos-automovil/10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PrqIngresoAutomovil>();
        Assert.NotNull(body);
        Assert.Equal((uint)10, body.Consecutivo);
    }

    [Fact]
    public async Task GetById_Returns_404_WhenNotFound()
    {
        await using var factory = new ApiWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/prq-ingresos-automovil/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // POST /api/prq-ingresos-automovil
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Create_Returns_201_With_CreatedIngreso()
    {
        await using var factory = new ApiWebApplicationFactory();
        var (scope, context) = factory.CreateDbContext();
        using (scope)
        {
            await SeedPrerequisitesAsync(context);
        }

        var client = factory.CreateClient();
        var request = new PrqIngresoAutomovilRequest(
            IdParqueo: 1,
            IdAutomovil: 1,
            FechaEntrada: new DateTime(2024, 3, 1, 8, 0, 0, DateTimeKind.Utc),
            FechaSalida: null);

        var response = await client.PostAsJsonAsync("/api/prq-ingresos-automovil", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PrqIngresoAutomovil>();
        Assert.NotNull(body);
        Assert.Equal((uint)1, body.IdParqueo);
        Assert.Equal((uint)1, body.IdAutomovil);
    }

    // -------------------------------------------------------------------------
    // PUT /api/prq-ingresos-automovil/{consecutivo}
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Update_Returns_204_WhenSuccess()
    {
        await using var factory = new ApiWebApplicationFactory();
        var (scope, context) = factory.CreateDbContext();
        using (scope)
        {
            await SeedPrerequisitesAsync(context);
            context.PrqIngresoAutomoviles.Add(new PrqIngresoAutomovil
            {
                Consecutivo = 20,
                IdParqueo = 1,
                IdAutomovil = 1,
                FechaEntrada = new DateTime(2024, 4, 1, 10, 0, 0, DateTimeKind.Utc)
            });
            await context.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        var request = new PrqIngresoAutomovilRequest(
            IdParqueo: 1,
            IdAutomovil: 1,
            FechaEntrada: new DateTime(2024, 4, 1, 10, 0, 0, DateTimeKind.Utc),
            FechaSalida: new DateTime(2024, 4, 1, 12, 30, 0, DateTimeKind.Utc));

        var response = await client.PutAsJsonAsync("/api/prq-ingresos-automovil/20", request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Update_Returns_404_WhenNotFound()
    {
        await using var factory = new ApiWebApplicationFactory();
        var client = factory.CreateClient();

        var request = new PrqIngresoAutomovilRequest(1, 1,
            new DateTime(2024, 1, 1, 8, 0, 0, DateTimeKind.Utc), null);

        var response = await client.PutAsJsonAsync("/api/prq-ingresos-automovil/8888", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // DELETE /api/prq-ingresos-automovil/{consecutivo}
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Delete_Returns_204_WhenSuccess()
    {
        await using var factory = new ApiWebApplicationFactory();
        var (scope, context) = factory.CreateDbContext();
        using (scope)
        {
            await SeedPrerequisitesAsync(context);
            context.PrqIngresoAutomoviles.Add(new PrqIngresoAutomovil
            {
                Consecutivo = 30,
                IdParqueo = 1,
                IdAutomovil = 1,
                FechaEntrada = new DateTime(2024, 5, 1, 8, 0, 0, DateTimeKind.Utc)
            });
            await context.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        var response = await client.DeleteAsync("/api/prq-ingresos-automovil/30");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Returns_404_WhenNotFound()
    {
        await using var factory = new ApiWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.DeleteAsync("/api/prq-ingresos-automovil/7777");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // GET /api/prq-ingresos-automovil/precio-por-hora/{idParqueo}
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ObtenerPrecioPorHora_Returns_200_With_Price()
    {
        await using var factory = new ApiWebApplicationFactory();
        var (scope, context) = factory.CreateDbContext();
        using (scope)
        {
            context.PrqParqueos.Add(new PrqParqueo
            {
                Id = 5,
                Provincia = "Guanacaste",
                Nombre = "Parqueo Playa",
                PrecioHora = 750m
            });
            await context.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/prq-ingresos-automovil/precio-por-hora/5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var price = await response.Content.ReadFromJsonAsync<decimal>();
        Assert.Equal(750m, price);
    }

    [Fact]
    public async Task ObtenerPrecioPorHora_Returns_404_WhenParqueoNotFound()
    {
        await using var factory = new ApiWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/prq-ingresos-automovil/precio-por-hora/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // GET /api/prq-ingresos-automovil/por-tipo-y-fecha
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByTipoAndDateRange_Returns_200_With_FilteredResults()
    {
        await using var factory = new ApiWebApplicationFactory();
        var (scope, context) = factory.CreateDbContext();
        using (scope)
        {
            context.PrqParqueos.Add(new PrqParqueo { Id = 10, Provincia = "Heredia", Nombre = "P", PrecioHora = 400m });
            context.PrqAutomoviles.AddRange(
                new PrqAutomovil { Id = 10, Color = "Rojo", Anio = 2022, Fabricante = "Toyota", Tipo = "Sedán" },
                new PrqAutomovil { Id = 11, Color = "Azul", Anio = 2021, Fabricante = "Ford", Tipo = "Pickup" });
            await context.SaveChangesAsync();

            context.PrqIngresoAutomoviles.AddRange(
                new PrqIngresoAutomovil
                {
                    Consecutivo = 100,
                    IdParqueo = 10,
                    IdAutomovil = 10, // Sedán
                    FechaEntrada = new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc),
                    FechaSalida = new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc)
                },
                new PrqIngresoAutomovil
                {
                    Consecutivo = 101,
                    IdParqueo = 10,
                    IdAutomovil = 11, // Pickup
                    FechaEntrada = new DateTime(2024, 6, 2, 8, 0, 0, DateTimeKind.Utc),
                    FechaSalida = new DateTime(2024, 6, 2, 9, 0, 0, DateTimeKind.Utc)
                });
            await context.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        var response = await client.GetAsync(
            "/api/prq-ingresos-automovil/por-tipo-y-fecha?tipo=Sed%C3%A1n&startDate=2024-06-01&endDate=2024-06-30");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<PrqIngresoAutomovilQueryResult>>();
        Assert.NotNull(body);
        Assert.Single(body);
        Assert.Equal((uint)100, body[0].Consecutivo);
    }

    [Fact]
    public async Task GetByTipoAndDateRange_Returns_400_WhenStartDateAfterEndDate()
    {
        await using var factory = new ApiWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/prq-ingresos-automovil/por-tipo-y-fecha?tipo=Sedán&startDate=2024-12-31&endDate=2024-01-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // GET /api/prq-ingresos-automovil/por-provincia-y-fecha
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByProvinciaAndDateRange_Returns_200_With_FilteredResults()
    {
        await using var factory = new ApiWebApplicationFactory();
        var (scope, context) = factory.CreateDbContext();
        using (scope)
        {
            context.PrqParqueos.AddRange(
                new PrqParqueo { Id = 20, Provincia = "Cartago", Nombre = "P-Cartago", PrecioHora = 300m },
                new PrqParqueo { Id = 21, Provincia = "Limón", Nombre = "P-Limón", PrecioHora = 250m });
            context.PrqAutomoviles.Add(
                new PrqAutomovil { Id = 20, Color = "Blanco", Anio = 2020, Fabricante = "Hyundai", Tipo = "SUV" });
            await context.SaveChangesAsync();

            context.PrqIngresoAutomoviles.AddRange(
                new PrqIngresoAutomovil
                {
                    Consecutivo = 200,
                    IdParqueo = 20, // Cartago
                    IdAutomovil = 20,
                    FechaEntrada = new DateTime(2024, 7, 10, 8, 0, 0, DateTimeKind.Utc),
                    FechaSalida = new DateTime(2024, 7, 10, 10, 0, 0, DateTimeKind.Utc)
                },
                new PrqIngresoAutomovil
                {
                    Consecutivo = 201,
                    IdParqueo = 21, // Limón
                    IdAutomovil = 20,
                    FechaEntrada = new DateTime(2024, 7, 11, 9, 0, 0, DateTimeKind.Utc),
                    FechaSalida = new DateTime(2024, 7, 11, 11, 0, 0, DateTimeKind.Utc)
                });
            await context.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        var response = await client.GetAsync(
            "/api/prq-ingresos-automovil/por-provincia-y-fecha?provincia=Cartago&startDate=2024-07-01&endDate=2024-07-31");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<PrqIngresoAutomovilQueryResult>>();
        Assert.NotNull(body);
        Assert.Single(body);
        Assert.Equal((uint)200, body[0].Consecutivo);
    }

    [Fact]
    public async Task GetByProvinciaAndDateRange_Returns_400_WhenStartDateAfterEndDate()
    {
        await using var factory = new ApiWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/prq-ingresos-automovil/por-provincia-y-fecha?provincia=Limón&startDate=2024-12-31&endDate=2024-01-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
