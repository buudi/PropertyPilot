namespace PropertyPilot.Services.CaretakerPortalServices.Models.Finances;

public class RecordExpenseCaretaker
{
    public required string Category { get; set; } = "No Category";
    public string? Description { get; set; }
    public required double Amount { get; set; }
}
