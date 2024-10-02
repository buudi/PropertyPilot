using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PropertyPilot.Dal.Models;

namespace PropertyPilot.Dal.Contexts;

public class PmsDbContext(IConfiguration configuration) : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // connects to postgres with the connection string from appsettings
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("pms"));
    }

    public DbSet<PropertyListing> PropertyListings { get; set; }
    public DbSet<PropertyPilotUser> PropertyPilotUsers { get; set; }
}
