using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace PropertyPilot.Dal.Contexts;

public class PmsDbContext(IConfiguration configuration) : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // connects to postgres with the connection string from appsettings
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("pms"));
    }


}
