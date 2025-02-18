using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.FinanceServices.Models;

namespace PropertyPilot.Services.Extensions;

public static class TransactionExtensions
{
    public static async Task<TransactionRecord> AsTransactionRecord(this Transaction transaction,
        PmsDbContext pmsDbContext)
    {
        var sourceAccountId = transaction.SourceAccountId;
        var destinationAccountId = transaction.DestinationAccountId;

        var record = new TransactionRecord();

        if (sourceAccountId != null)
        {
            var accountName = await pmsDbContext.MonetaryAccounts
                .Where(x => x.Id == sourceAccountId)
                .Select(x => x.AccountName)
                .FirstOrDefaultAsync();

            record.SourceAccountName = accountName;
        }

        if (destinationAccountId != null)
        {
            var accountName = await pmsDbContext.MonetaryAccounts
                .Where(x => x.Id == destinationAccountId)
                .Select(x => x.AccountName)
                .FirstOrDefaultAsync();

            record.DestinationAccountName = accountName;
        }

        record.PopulateTransactionObject(transaction);

        return record;
    }

}