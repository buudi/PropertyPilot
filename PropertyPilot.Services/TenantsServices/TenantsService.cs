using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;

namespace PropertyPilot.Services.TenantsServices;

public class TenantsService(PpDbContext ppDbContext)
{
    public async Task<List<Tenant>> GetAllTenantsAsync()
    {
        var tenants = await ppDbContext.Tenants.AsNoTracking().ToListAsync();

        return tenants;
    }

    public async Task<Tenant?> GetTenantByIdAsync(Guid id)
    {
        var tenant = await ppDbContext
            .Tenants
            .AsNoTracking()
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();

        return tenant;
    }
}
