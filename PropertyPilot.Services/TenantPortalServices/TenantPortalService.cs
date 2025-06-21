using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Services.Extensions;
using PropertyPilot.Services.TenantPortalServices.Models.Settings;

namespace PropertyPilot.Services.TenantPortalServices;

public class TenantPortalService(PmsDbContext pmsDbContext)
{
    public async Task<BasicTenantInfo?> GetBasicTenantInfo(Guid tenantAccountId)
    {
        var tenantAccount = await pmsDbContext
            .TenantAccounts
            .Where(x => x.Id == tenantAccountId)
            .FirstOrDefaultAsync();

        if (tenantAccount == null)
            return null;

        var tenantAccountRecord = await tenantAccount.GetTenantAccountRecord(pmsDbContext);

        return new BasicTenantInfo
        {
            Email = tenantAccountRecord.TenantAccount.Email,
            Name = tenantAccountRecord.Tenant.Name
        };
    }
}
