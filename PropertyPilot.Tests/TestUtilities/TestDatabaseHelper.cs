using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;

namespace PropertyPilot.Tests.TestUtilities;

public static class TestDatabaseHelper
{
    public static PmsDbContext CreatePmsDbContext(string databaseName = null)
    {
        var options = new DbContextOptionsBuilder<PmsDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        return new PmsDbContext(options);
    }

    public static PpDbContext CreatePpDbContext(string databaseName = null)
    {
        var options = new DbContextOptionsBuilder<PpDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        return new PpDbContext(options);
    }
} 