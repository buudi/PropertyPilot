using PropertyPilot.Dal.Models;

namespace PropertyPilot.Services.TenantsServices.Models;

public record BaseTenantRequest
{
    public required string Name { get; init; }
    public string? EmiratesId { get; init; }
    public required string PhoneNumber { get; init; }
    public string? Email { get; init; }
    public Contract? CurrentContract { get; init; }
    public required string LifecycleStatus { get; init; }
}
