namespace PropertyPilot.Services.CaretakerPortalServices.Models.Properties;

public class PaymentsTabListing
{
    public required Guid PaymentId { get; set; }
    public required string TenantName { get; set; }
    public required string SubUnitName { get; set; }
    public required DateTime PaymentDate { get; set; }
    public required string PaymentMethod { get; set; }
    public required double PaymentAmount { get; set; }
}
