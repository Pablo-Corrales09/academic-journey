using Microsoft.EntityFrameworkCore;
using ParqueoCarga.DbModel.Data;

namespace ParqueolCarga.RepositoryModel.Tests.TestUtilities;

internal static class RepositoryTestContextFactory
{
    public static ParqueoCargaContext Create()
    {
        var options = new DbContextOptionsBuilder<ParqueoCargaContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ParqueoCargaContext(options);
    }
}