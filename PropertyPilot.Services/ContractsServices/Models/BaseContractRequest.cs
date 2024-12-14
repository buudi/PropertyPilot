namespace PropertyPilot.Services.ContractsServices.Models;

public record BaseContractRequest
{
    public required Guid TenantId { get; set; }
    public required Guid PropertyId { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
    public required double Rent { get; set; }
    public string? Notes { get; set; }
    public required bool Active { get; set; }
    public required bool Renewable { get; set; }
    public required bool MoveOut { get; set; }
}
