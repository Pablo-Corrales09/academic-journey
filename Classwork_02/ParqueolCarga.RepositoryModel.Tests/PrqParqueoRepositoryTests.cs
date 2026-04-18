using ParqueoCarga.DbModel.Models;
using ParqueolCarga.RepositoryModel.Repositories;
using ParqueolCarga.RepositoryModel.Tests.TestUtilities;

namespace ParqueolCarga.RepositoryModel.Tests;

public class PrqParqueoRepositoryTests
{
    [Fact]
    public async Task Crud_Should_Create_Read_Update_And_Delete_PrqParqueo()
    {
        await using var context = RepositoryTestContextFactory.Create();
        var repository = new PrqParqueoRepository(context);

        var nuevo = new PrqParqueo
        {
            Id = 10,
            Provincia = "San José",
            Nombre = "Parqueo Centro",
            PrecioHora = 1200m
        };

        var creado = await repository.CreateAsync(nuevo);
        Assert.Equal((uint)10, creado.Id);

        var listado = await repository.GetAllAsync();
        Assert.Single(listado);

        var porId = await repository.GetByIdAsync(creado.Id);
        Assert.NotNull(porId);
        Assert.Equal("Parqueo Centro", porId!.Nombre);

        porId.PrecioHora = 1500m;
        var actualizado = await repository.UpdateAsync(porId);
        Assert.True(actualizado);

        var porIdActualizado = await repository.GetByIdAsync(creado.Id);
        Assert.NotNull(porIdActualizado);
        Assert.Equal(1500m, porIdActualizado!.PrecioHora);

        var eliminado = await repository.DeleteAsync(creado.Id);
        Assert.True(eliminado);

        var porIdEliminado = await repository.GetByIdAsync(creado.Id);
        Assert.Null(porIdEliminado);
    }
}