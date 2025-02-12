using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.FinanceServices.Models;

namespace PropertyPilot.Services.Extensions;

public static class RentPaymentExtensions
{
    public static async Task<RentPaymentTransactionRecord> AsRentPaymentRecord(this RentPayment rentPayment, PmsDbContext pmsDbContext)
    {
        var transaction = await pmsDbContext.Transactions
            .Where(x => x.TransactionType == Transaction.TransactionTypes.RentPayment)
            .Where(x => x.ReferenceId == rentPayment.Id)
            .FirstOrDefaultAsync();

        return new RentPaymentTransactionRecord { RentPayment = rentPayment, Transaction = transaction! };
    }
}