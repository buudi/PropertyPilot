using PropertyPilot.Dal.Models;
using PropertyPilot.Services.TenantServices.Models;

namespace PropertyPilot.Services.Extensions;

public static class TenantExtensions
{
    public static TenantListingRecord AsTenantListingRecord(this Tenant tenant)
    {
        return new TenantListingRecord
        {
            Id = tenant.Id,
            Name = tenant.Name,
            PhoneNumber = tenant.PhoneNumber,
            Email = tenant.Email,
            TenantIdentification = tenant.TenantIdentification,
            IsLeavingThisMonth = tenant.IsLeavingThisMonth,
            IsAccountActive = tenant.IsAccountActive
        };
    }
}
