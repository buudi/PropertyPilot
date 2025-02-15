namespace PropertyPilot.Services.FinanceServices.Models;

public record CreateExpenseRequest
{
    public Guid PropertyListingId { get; set; }
    public Guid PaidByUserId { get; set; }
    public string Category { get; set; } = "No Category";
    public string? Description { get; set; }
    public double Amount { get; set; }
}