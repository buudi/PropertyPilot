using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PropertyPilot.Dal.Models;

namespace PropertyPilot.Dal.Contexts;

public class PpDbContext(IConfiguration configuration) : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // connects to postgres with the connection string from appsettings
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("propertypilot"), b => b.MigrationsAssembly("PropertyPilot.Api"))
            .UseSnakeCaseNamingConvention();
    }

    public DbSet<Property> Properties { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Contract> Contracts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Property>()
            .Property(p => p.CreatedOn)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
