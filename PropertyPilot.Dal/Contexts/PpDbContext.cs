using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PropertyPilot.Dal.Models;

namespace PropertyPilot.Dal.Contexts;

public class PpDbContext(IConfiguration configuration) : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql(configuration.GetConnectionString("propertypilot"),
                       builder => builder.MigrationsAssembly("PropertyPilot.Api"))
            .UseSnakeCaseNamingConvention();
    }

    public DbSet<Property> Properties { get; set; }
    public DbSet<Contract> Contracts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Property>()
            .Property(p => p.CreatedOn)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<Contract>()
            .Property(p => p.CreatedOn)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<Contract>()
            .HasOne(c => c.Property) // Each Contract has one Property
            .WithMany(p => p.Contracts)  // Each Property can have many Contracts
            .HasForeignKey(c => c.PropertyId)
            .IsRequired();
    }
}
