namespace PropertyPilot.Services.TenantPortalServices.Models.Settings;

public class BasicTenantInfo
{
    public required string Name { get; set; }
    public required string Email { get; set; }
}

public class CaretakerDetailsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class RecentActivityDto
{
    public string Type { get; set; } = string.Empty; // e.g., "payment", "invoice", "lease"
    public DateTime Date { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? Amount { get; set; }
    public Guid? ReferenceId { get; set; }
}
