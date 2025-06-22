using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Services.CaretakerPortalServices;
using PropertyPilot.Services.CaretakerPortalServices.Models.Properties.TenantPage;
using PropertyPilot.Services.Extensions;
using PropertyPilot.Services.FinanceServices;
using PropertyPilot.Services.TenantPortalServices.Models.Settings;

namespace PropertyPilot.Services.TenantPortalServices;

public class TenantPortalService(
    PmsDbContext pmsDbContext,
    CaretakerPortalService caretakerPortalService,
    FinancesService financesService)
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

    public async Task<TenancyInformation?> GetCurrentActiveTenancyInfo(Guid tenantAccountId)
    {
        var tenantAccount = await pmsDbContext.TenantAccounts.FirstOrDefaultAsync(x => x.Id == tenantAccountId);
        if (tenantAccount == null || tenantAccount.TenantId == null)
            return null;

        var tenantAccountRecord = await tenantAccount.GetTenantAccountRecord(pmsDbContext);

        var activeTenancy = await pmsDbContext.Tenancies
            .Where(x => x.TenantId == tenantAccountRecord.Tenant.Id && x.IsTenancyActive)
            .OrderByDescending(x => x.TenancyStart)
            .FirstOrDefaultAsync();

        if (activeTenancy == null)
            return null;

        return await caretakerPortalService.GetTenancyInformation(activeTenancy.Id);
    }

    public async Task<double> GetOutstandingAmount(Guid tenantAccountId)
    {
        var tenantId = await pmsDbContext.TenantAccounts
            .Where(x => x.Id == tenantAccountId)
            .Select(x => x.TenantId)
            .FirstOrDefaultAsync();

        var tenantOutstanding = await financesService.IsTenantOutstanding((Guid)tenantId);

        return tenantOutstanding.OutstandingAmount;
    }

}
