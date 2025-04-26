using PropertyPilot.Dal.Models;

namespace PropertyPilot.Services.FinanceServices.Models;

public class TransactionListingRecord
{
    public TransactionRecord Transaction { get; set; }
    public RentPayment? RentPayment { get; set; }
    public Expense? Expense { get; set; }
}
