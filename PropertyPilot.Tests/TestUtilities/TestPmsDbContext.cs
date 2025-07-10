using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;

namespace PropertyPilot.Tests.TestUtilities;

public class TestPmsDbContext : PmsDbContext
{
    private readonly string _databaseName;

    public TestPmsDbContext(IConfiguration configuration, string databaseName = null) : base(configuration)
    {
        _databaseName = databaseName ?? Guid.NewGuid().ToString();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Use in-memory database for testing instead of PostgreSQL
        optionsBuilder.UseInMemoryDatabase(_databaseName);
    }
} 