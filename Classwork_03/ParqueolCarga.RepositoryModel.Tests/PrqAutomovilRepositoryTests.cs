using ParqueoCarga.DbModel.Models;
using ParqueolCarga.RepositoryModel.Repositories;
using ParqueolCarga.RepositoryModel.Tests.TestUtilities;

namespace ParqueolCarga.RepositoryModel.Tests;

public class PrqAutomovilRepositoryTests
{
    [Fact]
    public async Task Crud_Should_Create_Read_Update_And_Delete_PrqAutomovil()
    {
        await using var context = RepositoryTestContextFactory.Create();
        var repository = new PrqAutomovilRepository(context);

        var nuevo = new PrqAutomovil
        {
            Id = 1,
            Color = "Rojo",
            Anio = 2021,
            Fabricante = "Toyota",
            Tipo = "Sedán"
        };

        var creado = await repository.CreateAsync(nuevo);
        Assert.Equal((uint)1, creado.Id);

        var listado = await repository.GetAllAsync();
        Assert.Single(listado);

        var porId = await repository.GetByIdAsync(creado.Id);
        Assert.NotNull(porId);
        Assert.Equal("Rojo", porId!.Color);

        porId.Color = "Azul";
        var actualizado = await repository.UpdateAsync(porId);
        Assert.True(actualizado);

        var porIdActualizado = await repository.GetByIdAsync(creado.Id);
        Assert.NotNull(porIdActualizado);
        Assert.Equal("Azul", porIdActualizado!.Color);

        var eliminado = await repository.DeleteAsync(creado.Id);
        Assert.True(eliminado);

        var porIdEliminado = await repository.GetByIdAsync(creado.Id);
        Assert.Null(porIdEliminado);
    }
}