using System.Net;
using System.Net.Http.Json;
using ParqueoCarga.ApiModel.Dtos;
using ParqueoCarga.ApiModel.Tests.TestUtilities;
using ParqueoCarga.DbModel.Models;

namespace ParqueoCarga.ApiModel.Tests;

public class PrqParqueosControllerTests
{
    // -------------------------------------------------------------------------
    // GET /api/prq-parqueos
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAll_Returns_200_With_EmptyList_WhenNoneExist()
    {
        await using var factory = new ApiWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/prq-parqueos");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<PrqParqueo>>();
        Assert.NotNull(body);
        Assert.Empty(body);
    }

    [Fact]
    public async Task GetAll_Returns_200_With_SeededParqueos()
    {
        await using var factory = new ApiWebApplicationFactory();
        var (scope, context) = factory.CreateDbContext();
        using (scope)
        {
            context.PrqParqueos.AddRange(
                new PrqParqueo { Id = 1, Provincia = "San José", Nombre = "Parqueo Central", PrecioHora = 500m },
                new PrqParqueo { Id = 2, Provincia = "Alajuela", Nombre = "Parqueo Norte", PrecioHora = 400m });
            await context.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/prq-parqueos");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<PrqParqueo>>();
        Assert.NotNull(body);
        Assert.Equal(2, body.Count);
    }

    // -------------------------------------------------------------------------
    // GET /api/prq-parqueos/{id}
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetById_Returns_200_With_Parqueo_WhenExists()
    {
        await using var factory = new ApiWebApplicationFactory();
        var (scope, context) = factory.CreateDbContext();
        using (scope)
        {
            context.PrqParqueos.Add(
                new PrqParqueo { Id = 10, Provincia = "Heredia", Nombre = "Parqueo Sur", PrecioHora = 600m });
            await context.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/prq-parqueos/10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PrqParqueo>();
        Assert.NotNull(body);
        Assert.Equal("Heredia", body.Provincia);
    }

    [Fact]
    public async Task GetById_Returns_404_WhenNotFound()
    {
        await using var factory = new ApiWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/prq-parqueos/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // POST /api/prq-parqueos
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Create_Returns_201_With_CreatedParqueo()
    {
        await using var factory = new ApiWebApplicationFactory();
        var client = factory.CreateClient();

        var request = new PrqParqueoRequest("Cartago", "Parqueo Este", 350m);
        var response = await client.PostAsJsonAsync("/api/prq-parqueos", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PrqParqueo>();
        Assert.NotNull(body);
        Assert.Equal("Cartago", body.Provincia);
        Assert.Equal("Parqueo Este", body.Nombre);
        Assert.Equal(350m, body.PrecioHora);
    }

    // -------------------------------------------------------------------------
    // PUT /api/prq-parqueos/{id}
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Update_Returns_204_WhenSuccess()
    {
        await using var factory = new ApiWebApplicationFactory();
        var (scope, context) = factory.CreateDbContext();
        using (scope)
        {
            context.PrqParqueos.Add(
                new PrqParqueo { Id = 20, Provincia = "Limón", Nombre = "Parqueo Viejo", PrecioHora = 200m });
            await context.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        var request = new PrqParqueoRequest("Limón", "Parqueo Nuevo", 250m);
        var response = await client.PutAsJsonAsync("/api/prq-parqueos/20", request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Update_Returns_404_WhenNotFound()
    {
        await using var factory = new ApiWebApplicationFactory();
        var client = factory.CreateClient();

        var request = new PrqParqueoRequest("Limón", "Parqueo Inexistente", 100m);
        var response = await client.PutAsJsonAsync("/api/prq-parqueos/888", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // DELETE /api/prq-parqueos/{id}
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Delete_Returns_204_WhenSuccess()
    {
        await using var factory = new ApiWebApplicationFactory();
        var (scope, context) = factory.CreateDbContext();
        using (scope)
        {
            context.PrqParqueos.Add(
                new PrqParqueo { Id = 30, Provincia = "Puntarenas", Nombre = "Parqueo Playa", PrecioHora = 300m });
            await context.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        var response = await client.DeleteAsync("/api/prq-parqueos/30");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Returns_404_WhenNotFound()
    {
        await using var factory = new ApiWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.DeleteAsync("/api/prq-parqueos/777");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // GET /api/prq-parqueos/por-filtros
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByFiltros_Returns_200_Filtered_By_Provincia()
    {
        await using var factory = new ApiWebApplicationFactory();
        var (scope, context) = factory.CreateDbContext();
        using (scope)
        {
            context.PrqParqueos.AddRange(
                new PrqParqueo { Id = 40, Provincia = "San José", Nombre = "A", PrecioHora = 500m },
                new PrqParqueo { Id = 41, Provincia = "Alajuela", Nombre = "B", PrecioHora = 400m });
            await context.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/prq-parqueos/por-filtros?provincia=San+José");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<PrqParqueo>>();
        Assert.NotNull(body);
        Assert.Single(body);
        Assert.Equal("San José", body[0].Provincia);
    }

    [Fact]
    public async Task GetByFiltros_Returns_200_Filtered_By_PriceRange()
    {
        await using var factory = new ApiWebApplicationFactory();
        var (scope, context) = factory.CreateDbContext();
        using (scope)
        {
            context.PrqParqueos.AddRange(
                new PrqParqueo { Id = 50, Provincia = "X", Nombre = "Cheap", PrecioHora = 100m },
                new PrqParqueo { Id = 51, Provincia = "Y", Nombre = "Expensive", PrecioHora = 1000m });
            await context.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/prq-parqueos/por-filtros?minPrice=50&maxPrice=500");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<PrqParqueo>>();
        Assert.NotNull(body);
        Assert.Single(body);
        Assert.Equal("Cheap", body[0].Nombre);
    }
}
