using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.TenantServices.Models;

namespace PropertyPilot.Services.Extensions;

public static class TenantExtensions
{
    public static async Task<TenantListingRecord> AsTenantListingRecord(this Tenant tenant, PmsDbContext pmsDbContext)
    {
        var evacuationDate = await pmsDbContext.Tenancies
            .Where(x => x.TenantId == tenant.Id)
            .Select(x => x.EvacuationDate)
            .FirstOrDefaultAsync();

        bool isLeavingThisMonth = evacuationDate.HasValue &&
            evacuationDate.Value.Year == DateTime.UtcNow.Year &&
            evacuationDate.Value.Month == DateTime.UtcNow.Month;

        return new TenantListingRecord
        {
            Id = tenant.Id,
            Name = tenant.Name,
            PhoneNumber = tenant.PhoneNumber,
            Email = tenant.Email,
            TenantIdentification = tenant.TenantIdentification,
            IsLeavingThisMonth = isLeavingThisMonth,
            IsAccountActive = tenant.IsAccountActive
        };
    }
}
