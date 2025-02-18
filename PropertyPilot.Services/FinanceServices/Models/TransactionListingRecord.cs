using PropertyPilot.Dal.Models;

namespace PropertyPilot.Services.FinanceServices.Models;

public class TransactionListingRecord
{
    public TransactionRecord Transaction { get; set; }
    public RentPayment? RentPayment { get; set; }
    public Expense? Expense { get; set; }

    //public static async Task<TransactionListingRecord> CreateAsync(Transaction transaction, PmsDbContext pmsDbContext)
    //{
    //    var listing = new TransactionListingRecord(transaction);

    //    switch (transaction.TransactionType)
    //    {
    //        case Transaction.TransactionTypes.RentPayment:
    //            {
    //                var rentPayment = await pmsDbContext.RentPayments
    //                    .Where(x => x.Id == transaction.ReferenceId)
    //                    .FirstOrDefaultAsync();

    //                if (rentPayment != null)
    //                {
    //                    listing.RentPayment = rentPayment;
    //                }

    //                break;
    //            }
    //        case Transaction.TransactionTypes.Expense:
    //            {
    //                var expense = await pmsDbContext.Expenses
    //                    .Where(x => x.Id == transaction.ReferenceId)
    //                    .FirstOrDefaultAsync();

    //                if (expense != null)
    //                {
    //                    listing.Expense = expense;
    //                }

    //                break;
    //            }
    //    }

    //    return listing;
    //}
}
