using PropertyPilot.Dal.Abstractions;

namespace PropertyPilot.Services.TenantServices.Models;

public record TenantListingRecord : ITenant
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Email { get; set; }
    public required string TenantIdentification { get; set; }
    public required bool IsLeavingThisMonth { get; set; }
    public required bool IsAccountActive { get; set; }
}
