namespace PropertyPilot.Services.CaretakerPortalServices.Models.Properties.TenantPage;

public class TenantFinancialSummary
{
    public required double TotalOwed { get; set; }
    public required double TotalPaid { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public double? LastPaymentAmount { get; set; }
    public required string StayDuration { get; set; }
}