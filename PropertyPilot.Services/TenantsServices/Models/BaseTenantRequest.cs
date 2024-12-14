namespace PropertyPilot.Services.TenantsServices.Models;

public record BaseTenantRequest
{
    public required string Name { get; init; }
    public string? EmiratesId { get; init; }
    public required string PhoneNumber { get; init; }
    public string? Email { get; init; }
    public Guid? CurrentContractId { get; init; }
}
