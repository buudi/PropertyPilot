namespace PropertyPilot.Services.FinanceServices.Models;

public class IsTenantOutstanding
{
    public required bool IsOutstanding { get; set; }
    public required double OutstandingAmount { get; set; } = 0.0;
}
