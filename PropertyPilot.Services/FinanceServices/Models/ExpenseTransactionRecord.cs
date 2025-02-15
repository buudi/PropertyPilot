using PropertyPilot.Dal.Models;

namespace PropertyPilot.Services.FinanceServices.Models;

public record ExpenseTransactionRecord
{
    public Expense Expense { get; set; }
    public Transaction Transaction { get; set; }
}