using System.Net;
using System.Net.Http.Json;
using ParqueoCarga.ApiModel.Dtos;
using ParqueoCarga.ApiModel.Tests.TestUtilities;
using ParqueoCarga.DbModel.Models;

namespace ParqueoCarga.ApiModel.Tests;

public class PrqAutomovilesControllerTests
{
    // -------------------------------------------------------------------------
    // GET /api/prq-automoviles
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAll_Returns_200_With_EmptyList_WhenNoneExist()
    {
        await using var factory = new ApiWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/prq-automoviles");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<PrqAutomovil>>();
        Assert.NotNull(body);
        Assert.Empty(body);
    }

    [Fact]
    public async Task GetAll_Returns_200_With_SeededAutomoviles()
    {
        await using var factory = new ApiWebApplicationFactory();
        var (scope, context) = factory.CreateDbContext();
        using (scope)
        {
            context.PrqAutomoviles.AddRange(
                new PrqAutomovil { Id = 1, Color = "Rojo", Anio = 2020, Fabricante = "Toyota", Tipo = "Sedán" },
                new PrqAutomovil { Id = 2, Color = "Azul", Anio = 2021, Fabricante = "Honda", Tipo = "SUV" });
            await context.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/prq-automoviles");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<PrqAutomovil>>();
        Assert.NotNull(body);
        Assert.Equal(2, body.Count);
    }

    // -------------------------------------------------------------------------
    // GET /api/prq-automoviles/{id}
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetById_Returns_200_With_Automovil_WhenExists()
    {
        await using var factory = new ApiWebApplicationFactory();
        var (scope, context) = factory.CreateDbContext();
        using (scope)
        {
            context.PrqAutomoviles.Add(
                new PrqAutomovil { Id = 10, Color = "Verde", Anio = 2019, Fabricante = "Ford", Tipo = "Pickup" });
            await context.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/prq-automoviles/10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PrqAutomovil>();
        Assert.NotNull(body);
        Assert.Equal("Verde", body.Color);
    }

    [Fact]
    public async Task GetById_Returns_404_WhenNotFound()
    {
        await using var factory = new ApiWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/prq-automoviles/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // POST /api/prq-automoviles
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Create_Returns_201_With_CreatedAutomovil()
    {
        await using var factory = new ApiWebApplicationFactory();
        var client = factory.CreateClient();

        var request = new PrqAutomovilRequest("Negro", 2023, "BMW", "Coupé");
        var response = await client.PostAsJsonAsync("/api/prq-automoviles", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PrqAutomovil>();
        Assert.NotNull(body);
        Assert.Equal("Negro", body.Color);
        Assert.Equal(2023, body.Anio);
        Assert.Equal("BMW", body.Fabricante);
        Assert.Equal("Coupé", body.Tipo);
    }

    // -------------------------------------------------------------------------
    // PUT /api/prq-automoviles/{id}
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Update_Returns_204_WhenSuccess()
    {
        await using var factory = new ApiWebApplicationFactory();
        var (scope, context) = factory.CreateDbContext();
        using (scope)
        {
            context.PrqAutomoviles.Add(
                new PrqAutomovil { Id = 20, Color = "Blanco", Anio = 2015, Fabricante = "Kia", Tipo = "Hatchback" });
            await context.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        var request = new PrqAutomovilRequest("Gris", 2015, "Kia", "Hatchback");
        var response = await client.PutAsJsonAsync("/api/prq-automoviles/20", request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Update_Returns_404_WhenNotFound()
    {
        await using var factory = new ApiWebApplicationFactory();
        var client = factory.CreateClient();

        var request = new PrqAutomovilRequest("Plateado", 2020, "Nissan", "Sedán");
        var response = await client.PutAsJsonAsync("/api/prq-automoviles/8888", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // DELETE /api/prq-automoviles/{id}
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Delete_Returns_204_WhenSuccess()
    {
        await using var factory = new ApiWebApplicationFactory();
        var (scope, context) = factory.CreateDbContext();
        using (scope)
        {
            context.PrqAutomoviles.Add(
                new PrqAutomovil { Id = 30, Color = "Amarillo", Anio = 2018, Fabricante = "Chevrolet", Tipo = "Van" });
            await context.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        var response = await client.DeleteAsync("/api/prq-automoviles/30");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Returns_404_WhenNotFound()
    {
        await using var factory = new ApiWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.DeleteAsync("/api/prq-automoviles/7777");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // GET /api/prq-automoviles/por-color
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByPartialColor_Returns_200_With_MatchingAutomoviles()
    {
        await using var factory = new ApiWebApplicationFactory();
        var (scope, context) = factory.CreateDbContext();
        using (scope)
        {
            context.PrqAutomoviles.AddRange(
                new PrqAutomovil { Id = 40, Color = "Azul Oscuro", Anio = 2020, Fabricante = "Toyota", Tipo = "Sedán" },
                new PrqAutomovil { Id = 41, Color = "Rojo Vivo", Anio = 2021, Fabricante = "Honda", Tipo = "SUV" });
            await context.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/prq-automoviles/por-color?color=Azul");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<PrqAutomovil>>();
        Assert.NotNull(body);
        Assert.Single(body);
        Assert.Contains("Azul", body[0].Color);
    }

    // -------------------------------------------------------------------------
    // GET /api/prq-automoviles/por-anio
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByYearRange_Returns_200_With_MatchingAutomoviles()
    {
        await using var factory = new ApiWebApplicationFactory();
        var (scope, context) = factory.CreateDbContext();
        using (scope)
        {
            context.PrqAutomoviles.AddRange(
                new PrqAutomovil { Id = 50, Color = "Negro", Anio = 2010, Fabricante = "Ford", Tipo = "Sedán" },
                new PrqAutomovil { Id = 51, Color = "Blanco", Anio = 2022, Fabricante = "Kia", Tipo = "SUV" });
            await context.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/prq-automoviles/por-anio?startYear=2018&endYear=2024");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<PrqAutomovil>>();
        Assert.NotNull(body);
        Assert.Single(body);
        Assert.Equal(2022, body[0].Anio);
    }

    [Fact]
    public async Task GetByYearRange_Returns_400_WhenStartYearIsAfterEndYear()
    {
        await using var factory = new ApiWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/prq-automoviles/por-anio?startYear=2025&endYear=2000");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // GET /api/prq-automoviles/por-fabricante
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByPartialFabricante_Returns_200_With_MatchingAutomoviles()
    {
        await using var factory = new ApiWebApplicationFactory();
        var (scope, context) = factory.CreateDbContext();
        using (scope)
        {
            context.PrqAutomoviles.AddRange(
                new PrqAutomovil { Id = 60, Color = "Gris", Anio = 2020, Fabricante = "Toyota Corolla", Tipo = "Sedán" },
                new PrqAutomovil { Id = 61, Color = "Azul", Anio = 2021, Fabricante = "Honda Civic", Tipo = "Sedán" });
            await context.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/prq-automoviles/por-fabricante?fabricante=Toyota");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<PrqAutomovil>>();
        Assert.NotNull(body);
        Assert.Single(body);
        Assert.Contains("Toyota", body[0].Fabricante);
    }

    // -------------------------------------------------------------------------
    // GET /api/prq-automoviles/por-tipo
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByPartialTipo_Returns_200_With_MatchingAutomoviles()
    {
        await using var factory = new ApiWebApplicationFactory();
        var (scope, context) = factory.CreateDbContext();
        using (scope)
        {
            context.PrqAutomoviles.AddRange(
                new PrqAutomovil { Id = 70, Color = "Verde", Anio = 2020, Fabricante = "Jeep", Tipo = "SUV Grande" },
                new PrqAutomovil { Id = 71, Color = "Rojo", Anio = 2019, Fabricante = "Mini", Tipo = "Hatchback" });
            await context.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/prq-automoviles/por-tipo?tipo=SUV");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<PrqAutomovil>>();
        Assert.NotNull(body);
        Assert.Single(body);
        Assert.Contains("SUV", body[0].Tipo);
    }
}
