namespace PropertyPilot.Services.CaretakerPortalServices.Models.Properties;

public class SubunitsTabListing
{
    public required Guid SubunitId { get; set; }
    public required string SubunitName { get; set; }
    public required bool isOccupied { get; set; }
    public string? TenantName { get; set; }
}
