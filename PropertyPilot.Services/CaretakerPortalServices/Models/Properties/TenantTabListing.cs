namespace PropertyPilot.Services.CaretakerPortalServices.Models.Properties;

public class TenantTabListing
{
    public required string Name { get; set; }
    public required string UnitNumber { get; set; }
    public DateTime? LeaseEndDateTime { get; set; }
    public bool IsLeaseAutoRenewable { get; set; }
    public DateTime? NextLeaseRenewDate { get; set; }
    public required bool IsLeavingThisMonth { get; set; }
    public required bool HasOutstandingBalance { get; set; }
    public double? AmountDue { get; set; }
}