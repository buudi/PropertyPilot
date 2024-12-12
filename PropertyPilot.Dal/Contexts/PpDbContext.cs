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

    public DbSet<PropertiesList> PropertiesList { get; set; }
    public DbSet<Tenant> Tenant { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PropertiesList>()
            .Property(p => p.CreatedOn)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
