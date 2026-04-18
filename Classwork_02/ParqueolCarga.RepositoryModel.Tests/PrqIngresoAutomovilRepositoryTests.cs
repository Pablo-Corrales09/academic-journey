using ParqueoCarga.DbModel.Models;
using ParqueolCarga.RepositoryModel.Repositories;
using ParqueolCarga.RepositoryModel.Tests.TestUtilities;

namespace ParqueolCarga.RepositoryModel.Tests;

public class PrqIngresoAutomovilRepositoryTests
{
    [Fact]
    public async Task Crud_Should_Create_Read_Update_And_Delete_PrqIngresoAutomovil()
    {
        await using var context = RepositoryTestContextFactory.Create();

        context.PrqAutomoviles.Add(new PrqAutomovil
        {
            Id = 100,
            Color = "Negro",
            Anio = 2020,
            Fabricante = "Nissan",
            Tipo = "4x4"
        });

        context.PrqParqueos.Add(new PrqParqueo
        {
            Id = 200,
            Provincia = "Heredia",
            Nombre = "Parqueo Norte",
            PrecioHora = 1000m
        });

        await context.SaveChangesAsync();

        var repository = new PrqIngresoAutomovilRepository(context);

        var nuevo = new PrqIngresoAutomovil
        {
            Consecutivo = 300,
            IdAutomovil = 100,
            IdParqueo = 200,
            FechaEntrada = new DateTime(2026, 4, 1, 8, 0, 0),
            FechaSalida = new DateTime(2026, 4, 1, 10, 0, 0)
        };

        var creado = await repository.CreateAsync(nuevo);
        Assert.Equal((uint)300, creado.Consecutivo);

        var listado = await repository.GetAllAsync();
        Assert.Single(listado);

        var porId = await repository.GetByIdAsync(creado.Consecutivo);
        Assert.NotNull(porId);
        Assert.Equal((uint)200, porId!.IdParqueo);

        porId.FechaSalida = new DateTime(2026, 4, 1, 11, 0, 0);
        var actualizado = await repository.UpdateAsync(porId);
        Assert.True(actualizado);

        var porIdActualizado = await repository.GetByIdAsync(creado.Consecutivo);
        Assert.NotNull(porIdActualizado);
        Assert.Equal(new DateTime(2026, 4, 1, 11, 0, 0), porIdActualizado!.FechaSalida);

        var eliminado = await repository.DeleteAsync(creado.Consecutivo);
        Assert.True(eliminado);

        var porIdEliminado = await repository.GetByIdAsync(creado.Consecutivo);
        Assert.Null(porIdEliminado);
    }
}