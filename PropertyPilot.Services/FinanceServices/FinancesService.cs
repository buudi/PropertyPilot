using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.Constants;
using PropertyPilot.Services.Extensions;
using PropertyPilot.Services.FinanceServices.Models;
using PropertyPilot.Services.Generics;
using PropertyPilot.Services.InvoiceServices.Models;
using PropertyPilot.Services.TenantServices.Models;

namespace PropertyPilot.Services.FinanceServices;

public class FinancesService(PmsDbContext pmsDbContext, ILogger<FinancesService> logger)
{
    private const double Tolerance = 1.0;
    private readonly Guid _mainMonetaryAccountGuid = Keys.MainMonetaryAccountGuid;
    private readonly Guid _stripeMonetaryAccountGuid = Keys.StripeMonetaryAccountGuid;
    private readonly InvoiceDomainService _invoiceDomainService;

    public FinancesService(PmsDbContext pmsDbContext, ILogger<FinancesService> logger)
    {
        this.pmsDbContext = pmsDbContext;
        this.logger = logger;
        _invoiceDomainService = new InvoiceDomainService(pmsDbContext);
    }

    private async Task<bool> IsInvoicePaid(Invoice invoice)
    {
        var rentPaymentsSum = await pmsDbContext.RentPayments
            .Where(rentPayment => rentPayment.InvoiceId == invoice.Id)
            .SumAsync(rentPayment => rentPayment.Amount);

        var invoiceTotalAmount = await invoice.TotalAmountMinusDiscount(pmsDbContext);

        return rentPaymentsSum - Tolerance > invoiceTotalAmount;
    }


    private async Task<AttemptResult<MonetaryAccount>> UpdateAccountBalance(Transaction transaction)
    {
        var sourceAccount = transaction.SourceAccountId.HasValue
            ? await pmsDbContext
                .MonetaryAccounts
                .Where(x => x.Id == transaction.SourceAccountId.Value)
                .FirstOrDefaultAsync()
            : null;

        var destinationAccount = transaction.DestinationAccountId.HasValue
            ? await pmsDbContext
                .MonetaryAccounts
                .Where(x => x.Id == transaction.DestinationAccountId.Value)
                .FirstOrDefaultAsync()
            : null;

        if (sourceAccount != null)
        {
            if (sourceAccount.Balance < transaction.Amount - Tolerance)
            {
                return new AttemptResult<MonetaryAccount>(402, "Payment Required! Insufficient balance in the source account.");
            }

            sourceAccount.Balance -= transaction.Amount;
            //pmsDbContext.MonetaryAccounts.Update(sourceAccount);
        }

        if (destinationAccount != null)
        {
            destinationAccount.Balance += transaction.Amount;
            //pmsDbContext.MonetaryAccounts.Update(destinationAccount);
        }

        await pmsDbContext.SaveChangesAsync();

        return new AttemptResult<MonetaryAccount>(destinationAccount ?? sourceAccount!);
    }

    private async Task<TransactionListingRecord> CreateTransactionListingRecordAsync(Transaction transaction)
    {
        var listing = new TransactionListingRecord
        {
            Transaction = await transaction.AsTransactionRecord(pmsDbContext)
        };

        switch (transaction.TransactionType)
        {
            case Transaction.TransactionTypes.RentPayment:
                {
                    var rentPayment = await pmsDbContext.RentPayments
                        .Where(x => x.Id == transaction.ReferenceId)
                        .FirstOrDefaultAsync();

                    if (rentPayment != null)
                    {
                        listing.RentPayment = rentPayment;
                    }

                    break;
                }
            case Transaction.TransactionTypes.Expense:
                {
                    var expense = await pmsDbContext.Expenses
                        .Where(x => x.Id == transaction.ReferenceId)
                        .FirstOrDefaultAsync();

                    if (expense != null)
                    {
                        listing.Expense = expense;
                    }

                    break;
                }
        }

        return listing;
    }

    public async Task<AttemptResult<RentPaymentTransactionRecord>> RecordRentPayment(Guid userId, RentPaymentRequest request)
    {
        var invoice = await pmsDbContext
            .Invoices
            .Where(x => x.Id == request.InvoiceId)
            .FirstOrDefaultAsync();

        if (invoice == null)
        {
            return new AttemptResult<RentPaymentTransactionRecord>(404, "Invoice not found");
        }

        var isInvoicePaid = await IsInvoicePaid(invoice);

        if (isInvoicePaid)
        {
            return new AttemptResult<RentPaymentTransactionRecord>(409, "Conflict: Invoice already paid");
        }

        var receiverAccountId = request.PaymentMethod switch
        {
            RentPayment.PaymentMethods.Cash => await pmsDbContext.MonetaryAccounts
                .Where(account => account.UserId == userId)
                .Select(account => (Guid?)account.Id)
                .FirstOrDefaultAsync(),
            RentPayment.PaymentMethods.BankTransferToMain => _mainMonetaryAccountGuid,
            RentPayment.PaymentMethods.StripePayment => _stripeMonetaryAccountGuid,
            _ => null
        };

        if (receiverAccountId == null)
        {
            return new AttemptResult<RentPaymentTransactionRecord>(400, "Bad request: Invalid payment method");
        }

        await using var dbTransaction = await pmsDbContext.Database.BeginTransactionAsync();
        try
        {
            var rentPayment = new RentPayment
            {
                InvoiceId = request.InvoiceId,
                TenantId = request.TenantId,
                Amount = request.Amount,
                ReceiverAccountId = (Guid)receiverAccountId,
                PaymentMethod = request.PaymentMethod,
                AddedByUserId = userId
            };

            pmsDbContext.RentPayments.Add(rentPayment);
            await pmsDbContext.SaveChangesAsync();

            var transaction = new Transaction
            {
                TransactionType = Transaction.TransactionTypes.RentPayment,
                ReferenceId = rentPayment.Id,
                SourceAccountId = null,
                DestinationAccountId = (Guid)receiverAccountId,
                Amount = request.Amount
            };

            pmsDbContext.Transactions.Add(transaction);
            await pmsDbContext.SaveChangesAsync();

            var updateBalanceAttempt = await UpdateAccountBalance(transaction);
            if (!updateBalanceAttempt.IsSuccess)
            {
                await dbTransaction.RollbackAsync();
                return new AttemptResult<RentPaymentTransactionRecord>(updateBalanceAttempt.ErrorCode, updateBalanceAttempt.ErrorMessage);
            }

            await _invoiceDomainService.UpdateInvoiceStatusAsync(request.InvoiceId);

            await dbTransaction.CommitAsync();

            return new AttemptResult<RentPaymentTransactionRecord>(new RentPaymentTransactionRecord
            {
                RentPayment = rentPayment,
                Transaction = transaction
            });
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    //public async Task<List<RentPaymentTransactionRecord>> GetRentPaymentTransactionRecordForInvoice(Guid invoiceId)
    //{
    //    var rentPayments = await pmsDbContext.RentPayments.Where(rentPayment => rentPayment.InvoiceId == invoiceId)
    //        .ToListAsync();

    //    var transactionIds = await pmsDbContext.Transactions
    //        .Where(transaction =>
    //            transaction
    //            .TransactionType == "RentPayment"
    //                && rentPayments
    //                    .Select(rp => rp.Id)
    //                    .Contains(transaction.ReferenceId))
    //                    .Select(transaction => transaction.Id)
    //                    .ToListAsync();

    //    var transactions = await pmsDbContext.Transactions.Where(transaction => transactionIds.Contains(transaction.Id))
    //        .ToListAsync();

    //    var rentPaymentTransactionRecords = rentPayments.Select(rentPayment => new RentPaymentTransactionRecord
    //    {
    //        RentPayment = rentPayment,
    //        Transaction = transactions.FirstOrDefault(transaction => transaction.ReferenceId == rentPayment.Id)!
    //    })
    //        .ToList();

    //    return rentPaymentTransactionRecords;
    //}

    public async Task<List<RentPaymentTransactionRecord>> GetRentPaymentTransactionRecordForInvoice(Guid invoiceId)
    {
        var records = await (
            from rp in pmsDbContext.RentPayments
            where rp.InvoiceId == invoiceId
            join t in pmsDbContext.Transactions
                on rp.Id equals t.ReferenceId into tGroup
            from t in tGroup.DefaultIfEmpty()
            select new RentPaymentTransactionRecord
            {
                RentPayment = rp,
                Transaction = t
            }
        ).ToListAsync();

        return records;
    }

    public async Task<RentPaymentTransactionRecord?> GetRentPaymentTransactionRecordByPaymentId(Guid paymentId)
    {
        var rentPayment = await pmsDbContext.RentPayments
            .Where(x => x.Id == paymentId)
            .FirstOrDefaultAsync();

        if (rentPayment == null)
        {
            return null;
        }

        var record = await rentPayment.AsRentPaymentRecord(pmsDbContext);

        return record;
    }

    public async Task<AttemptResult<ExpenseTransactionRecord>> RecordExpenseAsync(CreateExpenseRequest createExpenseRequest)
    {
        var paidByAccountId = await pmsDbContext.MonetaryAccounts
            .Where(x => x.UserId == createExpenseRequest.PaidByUserId)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        var expense = new Expense
        {
            PropertyListingId = createExpenseRequest.PropertyListingId,
            PaidByAccountId = paidByAccountId,
            PaidByUserId = createExpenseRequest.PaidByUserId,
            Category = createExpenseRequest.Category,
            Description = createExpenseRequest.Description,
            Amount = createExpenseRequest.Amount
        };

        await using var dbTransaction = await pmsDbContext.Database.BeginTransactionAsync();
        try
        {
            pmsDbContext.Expenses.Add(expense);
            await pmsDbContext.SaveChangesAsync();

            var transaction = new Transaction
            {
                TransactionType = Transaction.TransactionTypes.Expense,
                ReferenceId = expense.Id,
                SourceAccountId = paidByAccountId,
                Amount = createExpenseRequest.Amount
            };
            pmsDbContext.Transactions.Add(transaction);
            await pmsDbContext.SaveChangesAsync();

            var updateBalanceAttempt = await UpdateAccountBalance(transaction);
            if (!updateBalanceAttempt.IsSuccess)
            {
                await dbTransaction.RollbackAsync();
                return new AttemptResult<ExpenseTransactionRecord>(updateBalanceAttempt.ErrorCode, updateBalanceAttempt.ErrorMessage);
            }


            await dbTransaction.CommitAsync();

            var expenseRecord = await expense.AsExpenseTransactionRecord(pmsDbContext);
            return new AttemptResult<ExpenseTransactionRecord>(expenseRecord);
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
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

        await using var dbTransaction = await pmsDbContext.Database.BeginTransactionAsync();
        try
        {
            pmsDbContext.Transactions.Add(transaction);
            await pmsDbContext.SaveChangesAsync();

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

    public async Task<IsTenantOutstanding> IsTenantOutstanding(Guid tenantId)
    {
        var invoices = await pmsDbContext.Invoices
            .Where(x => x.TenantId == tenantId)
            .ToListAsync();

        var outstandingInvoices = invoices
            .Where(x => x.InvoiceStatus == Invoice.InvoiceStatuses.Outstanding || x.InvoiceStatus == Invoice.InvoiceStatuses.Pending)
            .ToList();

        var totalOutstandingAmount = 0.0;
        foreach (var invoice in outstandingInvoices)
        {
            var invoiceTotalAmount = await _invoiceDomainService.GetTotalAmountMinusDiscountAsync(invoice);
            totalOutstandingAmount += invoiceTotalAmount;
        }

        var isTenantOutstanding = new IsTenantOutstanding
        {
            IsOutstanding = outstandingInvoices.Count > 0,
            OutstandingAmount = totalOutstandingAmount
        };

        return isTenantOutstanding;
    }

    public async Task<double> TenantOutstandingSum(Guid tenancyId)
    {
        var invoices = await pmsDbContext.Invoices
            .Where(inv => inv.TenancyId == tenancyId &&
                          (inv.InvoiceStatus == Invoice.InvoiceStatuses.Pending || inv.InvoiceStatus == Invoice.InvoiceStatuses.Outstanding))
            .ToListAsync();

        double totalOutstanding = 0.0;
        foreach (var invoice in invoices)
        {
            totalOutstanding += await _invoiceDomainService.GetTotalAmountRemainingAsync(invoice);
        }

        return totalOutstanding;
    }

    public async Task<double> TenantTotalPaidRent(Guid tenancyId)
    {
        var invoiceIds = await pmsDbContext.Invoices
            .Where(inv => inv.TenancyId == tenancyId)
            .Select(inv => inv.Id)
            .ToListAsync();

        var totalPaid = await pmsDbContext.RentPayments
            .Where(rp => rp.InvoiceId.HasValue && invoiceIds.Contains(rp.InvoiceId.Value))
            .SumAsync(rp => rp.Amount);

        return totalPaid;
    }

    public async Task<(DateTime LastPaymentDate, double LastPaymentAmount)> TenantLastPaidRentDateAndAmount(Guid tenancyId)
    {
        var invoiceIds = await pmsDbContext.Invoices
            .Where(inv => inv.TenancyId == tenancyId)
            .Select(inv => inv.Id)
            .ToListAsync();

        var lastPayment = await pmsDbContext.RentPayments
            .Where(rp => rp.InvoiceId.HasValue && invoiceIds.Contains(rp.InvoiceId.Value))
            .OrderByDescending(rp => rp.CreatedAt)
            .FirstOrDefaultAsync();

        if (lastPayment == null)
        {
            return (DateTime.MinValue, 0.0);
        }

        return (lastPayment.CreatedAt, lastPayment.Amount);
    }


    public async Task<double> PropertyOutstandingSum(Guid propertyId)
    {
        var tenancies = await pmsDbContext.Tenancies
            .Where(t => t.PropertyListingId == propertyId)
            .Select(t => t.Id)
            .ToListAsync();

        if (!tenancies.Any())
            return 0.0;

        var invoices = await pmsDbContext.Invoices
            .Where(inv => tenancies.Contains(inv.TenancyId) &&
                          (inv.InvoiceStatus == Invoice.InvoiceStatuses.Pending || inv.InvoiceStatus == Invoice.InvoiceStatuses.Outstanding))
            .ToListAsync();

        double totalOutstanding = 0.0;
        foreach (var invoice in invoices)
        {
            totalOutstanding += await _invoiceDomainService.GetTotalAmountRemainingAsync(invoice);
        }

        return totalOutstanding;
    }
}
