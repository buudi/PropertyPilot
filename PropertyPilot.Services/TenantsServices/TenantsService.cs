using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.TenantsServices.Models;

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

    public async Task<Tenant> CreateTenantBasicInfo(CreateTenantRequest newTenantRequest)
    {
        var newTenant = new Tenant
        {
            Name = newTenantRequest.Name,
            EmiratesId = newTenantRequest.EmiratesId,
            PhoneNumber = newTenantRequest.PhoneNumber,
            Email = newTenantRequest.Email,
            LifecycleStatus = Tenant.LifecycleStatuses.Testing
        };

        ppDbContext.Tenants.Add(newTenant);
        await ppDbContext.SaveChangesAsync();
        return newTenant;
    }
}
