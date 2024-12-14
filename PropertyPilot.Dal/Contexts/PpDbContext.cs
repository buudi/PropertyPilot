using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PropertyPilot.Dal.Models;

namespace PropertyPilot.Dal.Contexts;

public class PpDbContext(IConfiguration configuration) : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // connects to postgres with the connection string from appsettings
        optionsBuilder
            .UseNpgsql(configuration.GetConnectionString("propertypilot"),
                       builder => builder.MigrationsAssembly("PropertyPilot.Api"))
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

        modelBuilder.Entity<Tenant>()
            .Property(p => p.CreatedOn)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<Contract>()
            .Property(p => p.CreatedOn)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // required one-to-one relationship between Contract and Tenant
        modelBuilder.Entity<Contract>()
            .HasOne(c => c.Tenant)
            .WithMany() // no back-navigation from Tenant to this Contract
            .HasForeignKey(c => c.TenantId)
            .IsRequired();

        // required one-to-one relationship between Contract and Property
        modelBuilder.Entity<Contract>()
            .HasOne(c => c.Property)
            .WithMany() // no back-navigation from Property to this Contract
            .HasForeignKey(c => c.PropertyId)
            .IsRequired();

        // optional one-to-one relationship between Tenant and CurrentContract
        modelBuilder.Entity<Tenant>()
            .HasOne(t => t.CurrentContract)
            .WithOne()
            .HasForeignKey<Tenant>(t => t.CurrentContractId)
            .IsRequired(false);
    }
}
