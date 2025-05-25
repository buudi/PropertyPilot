namespace PropertyPilot.Services.CaretakerPortalServices.Models.Properties.TenantPage;

public class TenancyInformation
{
    public Guid TenancyId { get; set; }
    public required string TenantName { get; set; }
    public required double AssignedRent { get; set; }
    public required string PropertyName { get; set; }
    public string? SubUnitName { get; set; } = string.Empty;
    public required bool IsTenancyActive { get; set; } // whether active or not
    public required string TenancyType { get; set; } // whether fixed term (with end date), or Renewable with renewal period
    public int? RenewalPeriodInDays { get; set; }
    public required string TenantPhoneNumber { get; set; }
    public required string TenantEmail { get; set; }
    public required DateTime TenancyStart { get; set; }
    public DateTime? TenancyEnd { get; set; }
    public DateTime? EvacuationDate { get; set; }
}