using Microsoft.Extensions.Configuration;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Tests.TestUtilities;

namespace PropertyPilot.Tests.TestUtilities;

public static class TestDatabaseHelper
{
    public static TestPmsDbContext CreatePmsDbContext(string databaseName = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:pms"] = "test-connection-string"
            })
            .Build();

        return new TestPmsDbContext(configuration, databaseName);
    }
} 