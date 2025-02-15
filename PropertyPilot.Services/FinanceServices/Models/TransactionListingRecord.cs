using PropertyPilot.Dal.Models;

namespace PropertyPilot.Services.FinanceServices.Models;

public record TransactionListingRecord
{
    public required Transaction Transaction { get; init; }
    public RentPayment? RentPayment { get; init; }
    public Expense? Expense { get; init; }
}