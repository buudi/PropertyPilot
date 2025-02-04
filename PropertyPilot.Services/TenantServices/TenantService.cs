using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.Extensions;
using PropertyPilot.Services.Generics;
using PropertyPilot.Services.TenantServices.Models;

namespace PropertyPilot.Services.TenantServices;

public class TenantService(PmsDbContext pmsDbContext)
{
    public async Task<PaginatedResult<TenantListingRecord>> GetAllTenantsListingAsync(int pageNumber, int pageSize)
    {

        var totalTenants = await pmsDbContext.Tenants.CountAsync();

        var tenants = await pmsDbContext.Tenants
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var tenantsListing = tenants
        .Select(t => t.AsTenantListingRecord())
        .ToList();

        return new PaginatedResult<TenantListingRecord>
        {
            Items = tenantsListing,
            TotalItems = totalTenants,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalTenants / (double)pageSize)
        };
    }

    public async Task<TenantListingRecord?> GetTenantRecordAsync(Guid tenantId)
    {
        var tenant = await pmsDbContext.Tenants
            .Where(x => x.Id == tenantId)
            .FirstOrDefaultAsync();

        if (tenant == null)
        {
            return null;
        }

        return tenant.AsTenantListingRecord();
    }

    public async Task<TenantListingRecord> CreateTenantAsync(TenantCreateRequest tenantCreateRequest)
    {
        var newTenant = new Tenant
        {
            Name = tenantCreateRequest.Name,
            PhoneNumber = tenantCreateRequest.PhoneNumber,
            Email = tenantCreateRequest.Email,
            TenantIdentification = tenantCreateRequest.TenantIdentification
        };

        pmsDbContext.Tenants.Add(newTenant);
        await pmsDbContext.SaveChangesAsync();

        return newTenant.AsTenantListingRecord();
    }

}
