using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.TenantPortalServices.Models;

namespace PropertyPilot.Services.Extensions;

public static class TenantAccountExtensions
{
    public static async Task<TenantAccountRecord> GetTenantAccountRecord(this TenantAccount tenantAccount, PmsDbContext pmsDbContext)
    {
        var tenant = await pmsDbContext
            .Tenants
            .Where(x => x.Id == tenantAccount.TenantId)
            .FirstOrDefaultAsync();

        return new TenantAccountRecord
        {
            TenantAccount = tenantAccount,
            Tenant = tenant!
        };
    }
}