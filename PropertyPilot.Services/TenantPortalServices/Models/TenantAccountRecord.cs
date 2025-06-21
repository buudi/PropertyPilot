using PropertyPilot.Dal.Models;

namespace PropertyPilot.Services.TenantPortalServices.Models;

public class TenantAccountRecord
{
    public required TenantAccount TenantAccount { get; set; }
    public required Tenant Tenant { get; set; }
}
