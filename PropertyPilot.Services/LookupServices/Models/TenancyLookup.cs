namespace PropertyPilot.Services.LookupServices.Models;

public class TenancyLookup
{
    public Guid Id { get; set; }
    public string PropertyListingName { get; set; } = string.Empty;
    public string? SubUnitIdentifierName { get; set; }
    public DateTime TenancyStart { get; set; }
    public DateTime? TenancyEnd { get; set; }
    public bool IsMonthlyRenewable { get; set; } = false;
    public bool IsTenancyActive { get; set; } = false;
    public DateTime? EvacuationDate { get; set; }
}
