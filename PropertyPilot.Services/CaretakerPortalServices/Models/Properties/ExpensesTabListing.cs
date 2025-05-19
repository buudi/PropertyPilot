namespace PropertyPilot.Services.CaretakerPortalServices.Models.Properties;

public class ExpensesTabListing
{
    public required Guid ExpenseId { get; set; }
    public string? ExpenseDescription { get; set; }
    public required DateTime ExpenseDate { get; set; }
    public required double Amount { get; set; }
    public required string Category { get; set; }
}
