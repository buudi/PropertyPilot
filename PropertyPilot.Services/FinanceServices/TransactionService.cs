using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.FinanceServices.Models;
using PropertyPilot.Services.Generics;

namespace PropertyPilot.Services.FinanceServices;

public class TransactionService
{
    private readonly PmsDbContext _db;
    private const double Tolerance = 1.0;

    public TransactionService(PmsDbContext db)
    {
        _db = db;
    }

    public async Task<AttemptResult<MonetaryAccount>> UpdateAccountBalance(Transaction transaction)
    {
        var sourceAccount = transaction.SourceAccountId.HasValue
            ? await _db.MonetaryAccounts.Where(x => x.Id == transaction.SourceAccountId.Value).FirstOrDefaultAsync()
            : null;
        var destinationAccount = transaction.DestinationAccountId.HasValue
            ? await _db.MonetaryAccounts.Where(x => x.Id == transaction.DestinationAccountId.Value).FirstOrDefaultAsync()
            : null;
        if (sourceAccount != null)
        {
            if (sourceAccount.Balance < transaction.Amount - Tolerance)
            {
                return new AttemptResult<MonetaryAccount>(402, "Payment Required! Insufficient balance in the source account.");
            }
            sourceAccount.Balance -= transaction.Amount;
        }
        if (destinationAccount != null)
        {
            destinationAccount.Balance += transaction.Amount;
        }
        await _db.SaveChangesAsync();
        return new AttemptResult<MonetaryAccount>(destinationAccount ?? sourceAccount!);
    }

    public async Task<TransactionListingRecord> CreateTransactionListingRecordAsync(Transaction transaction)
    {
        var listing = new TransactionListingRecord
        {
            Transaction = await transaction.AsTransactionRecord(_db)
        };
        switch (transaction.TransactionType)
        {
            case Transaction.TransactionTypes.RentPayment:
                var rentPayment = await _db.RentPayments.Where(x => x.Id == transaction.ReferenceId).FirstOrDefaultAsync();
                if (rentPayment != null)
                {
                    listing.RentPayment = rentPayment;
                }
                break;
            case Transaction.TransactionTypes.Expense:
                var expense = await _db.Expenses.Where(x => x.Id == transaction.ReferenceId).FirstOrDefaultAsync();
                if (expense != null)
                {
                    listing.Expense = expense;
                }
                break;
        }
        return listing;
    }

    public async Task<PaginatedResult<TransactionListingRecord>> GetTransactionsListingsAsync(int pageNumber, int pageSize)
    {
        var query = _db.Transactions;
        var transactions = await query.OrderByDescending(x => x.CreatedAt).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        var totalItems = await query.CountAsync();
        var transactionListings = new List<TransactionListingRecord>();
        foreach (var transaction in transactions)
        {
            var transactionListing = await CreateTransactionListingRecordAsync(transaction);
            transactionListings.Add(transactionListing);
        }
        return new PaginatedResult<TransactionListingRecord>
        {
            Items = transactionListings,
            TotalItems = totalItems,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
        };
    }

    public async Task<AttemptResult<Transaction>> RecordTransferAsync(CreateTransferRequest request)
    {
        var transaction = new Transaction
        {
            TransactionType = Transaction.TransactionTypes.Transfer,
            ReferenceId = Guid.Empty,
            SourceAccountId = request.SourceAccountId,
            DestinationAccountId = request.DestinationAccountId,
            Amount = request.Amount
        };
        await using var dbTransaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Transactions.Add(transaction);
            await _db.SaveChangesAsync();
            var updateBalanceAttempt = await UpdateAccountBalance(transaction);
            if (!updateBalanceAttempt.IsSuccess)
            {
                await dbTransaction.RollbackAsync();
                return new AttemptResult<Transaction>(updateBalanceAttempt.ErrorCode, updateBalanceAttempt.ErrorMessage);
            }
            await dbTransaction.CommitAsync();
            return new AttemptResult<Transaction>(transaction);
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }
} 