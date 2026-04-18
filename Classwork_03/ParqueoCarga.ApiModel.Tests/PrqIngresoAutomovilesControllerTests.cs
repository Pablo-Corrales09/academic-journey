using Microsoft.AspNetCore.Mvc;
using ParqueoCarga.ApiModel.Controllers;
using ParqueoCarga.DbModel.Models;
using ParqueolCarga.RepositoryModel.Dtos;
using ParqueolCarga.RepositoryModel.Interfaces;

namespace ParqueoCarga.ApiModel.Tests;

public class PrqIngresoAutomovilesControllerTests
{
    [Fact]
    public async Task GetByProvinciaAndDateRange_Should_Return_Ok_With_Results()
    {
        var expected = new List<PrqIngresoAutomovilQueryResult>
        {
            new()
            {
                Consecutivo = 1,
                IdParqueo = 10,
                IdAutomovil = 20,
                FechaEntrada = new DateTime(2026, 1, 10, 8, 0, 0),
                FechaSalida = new DateTime(2026, 1, 10, 10, 0, 0),
                MontoTotalPagar = 2400m
            }
        };

        var repository = new FakeIngresoRepository
        {
            ProvinciaDateRangeResult = expected
        };

        var controller = new PrqIngresoAutomovilesController(repository);

        var actionResult = await controller.GetByProvinciaAndDateRange(
            provincia: "Heredia",
            startDate: new DateTime(2026, 1, 1),
            endDate: new DateTime(2026, 1, 31),
            cancellationToken: CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var result = Assert.IsType<List<PrqIngresoAutomovilQueryResult>>(ok.Value);
        Assert.Single(result);
        Assert.Equal((uint)1, result[0].Consecutivo);
    }

    [Fact]
    public async Task GetById_Should_Return_NotFound_When_Entity_Does_Not_Exist()
    {
        var controller = new PrqIngresoAutomovilesController(new FakeIngresoRepository());

        var actionResult = await controller.GetById(999, CancellationToken.None);

        Assert.IsType<NotFoundResult>(actionResult.Result);
    }

    [Fact]
    public async Task Create_Should_Return_CreatedAtAction()
    {
        var repository = new FakeIngresoRepository();
        var controller = new PrqIngresoAutomovilesController(repository);

        var entity = new PrqIngresoAutomovil
        {
            Consecutivo = 12,
            IdParqueo = 3,
            IdAutomovil = 7,
            FechaEntrada = new DateTime(2026, 4, 1, 9, 0, 0)
        };

        var actionResult = await controller.Create(entity, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var createdEntity = Assert.IsType<PrqIngresoAutomovil>(created.Value);
        Assert.Equal((uint)12, createdEntity.Consecutivo);
        Assert.Equal((uint)12, repository.LastCreated!.Consecutivo);
    }

    private sealed class FakeIngresoRepository : IPrqIngresoAutomovilRepository
    {
        private readonly Dictionary<uint, PrqIngresoAutomovil> _items = new();

        public List<PrqIngresoAutomovilQueryResult> ProvinciaDateRangeResult { get; set; } = new();

        public PrqIngresoAutomovil? LastCreated { get; private set; }

        public Task<List<PrqIngresoAutomovil>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_items.Values.ToList());

        public Task<PrqIngresoAutomovil?> GetByIdAsync(uint consecutivo, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.GetValueOrDefault(consecutivo));

        public Task<PrqIngresoAutomovil> CreateAsync(PrqIngresoAutomovil entity, CancellationToken cancellationToken = default)
        {
            _items[entity.Consecutivo] = entity;
            LastCreated = entity;
            return Task.FromResult(entity);
        }

        public Task<bool> UpdateAsync(PrqIngresoAutomovil entity, CancellationToken cancellationToken = default)
        {
            if (!_items.ContainsKey(entity.Consecutivo))
            {
                return Task.FromResult(false);
            }

            _items[entity.Consecutivo] = entity;
            return Task.FromResult(true);
        }

        public Task<bool> DeleteAsync(uint consecutivo, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.Remove(consecutivo));

        public Task<decimal?> ObtenerPrecioPorHoraPorParqueo(uint idParqueo, CancellationToken cancellationToken = default)
            => Task.FromResult<decimal?>(1200m);

        public Task<List<PrqIngresoAutomovilQueryResult>> GetByTipoAndDateRange(
            string tipo,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new List<PrqIngresoAutomovilQueryResult>());

        public Task<List<PrqIngresoAutomovilQueryResult>> GetByProvinciaAndDateRange(
            string provincia,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
            => Task.FromResult(ProvinciaDateRangeResult);
    }
}
