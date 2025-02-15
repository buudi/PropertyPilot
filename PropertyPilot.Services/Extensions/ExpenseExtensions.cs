using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.FinanceServices.Models;

namespace PropertyPilot.Services.Extensions;

public static class ExpenseExtensions
{
    public static async Task<ExpenseTransactionRecord> AsExpenseTransactionRecord(this Expense expense,
        PmsDbContext pmsDbContext)
    {
        var transaction = await pmsDbContext.Transactions
            .Where(x => x.TransactionType == Transaction.TransactionTypes.Expense)
            .Where(x => x.ReferenceId == expense.Id)
            .FirstOrDefaultAsync();

        return new ExpenseTransactionRecord
        {
            Expense = expense,
            Transaction = transaction!
        };
    }
}