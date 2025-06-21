using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Services.CaretakerPortalServices;
using PropertyPilot.Services.CaretakerPortalServices.Models.Properties.TenantPage;
using PropertyPilot.Services.Extensions;
using PropertyPilot.Services.TenantPortalServices.Models.Settings;

namespace PropertyPilot.Services.TenantPortalServices;

public class TenantPortalService
{
    private readonly PmsDbContext pmsDbContext;
    private readonly CaretakerPortalService caretakerPortalService;

    public TenantPortalService(PmsDbContext pmsDbContext, CaretakerPortalService caretakerPortalService)
    {
        this.pmsDbContext = pmsDbContext;
        this.caretakerPortalService = caretakerPortalService;
    }

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
}
