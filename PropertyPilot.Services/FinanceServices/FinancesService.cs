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

    public async Task<Invoice?> GetInvoiceByIdAsync(Guid invoiceId)
    {
        var invoice = await pmsDbContext
            .Invoices
            .Where(x => x.Id == invoiceId)
            .FirstOrDefaultAsync();

        return invoice ?? null;
    }

    public async Task<InvoiceRecord> CreateTenancyAndInvoiceOnTenantCreate(Guid tenantId, TenantCreateRequest tenantCreateRequest, CreateInvoiceOnNewTenantRequest createInvoiceRequest)
    {

        var tenancyObject = new Tenancy
        {
            TenantId = tenantId,
            PropertyListingId = tenantCreateRequest.PropertyUnitId,
            SubUnitId = tenantCreateRequest.SubUnitId,
            AssignedRent = tenantCreateRequest.AssignedRent,
            TenancyStart = createInvoiceRequest.DateStart,
            TenancyEnd = tenantCreateRequest.TenancyEnd,
            IsRenewable = tenantCreateRequest.IsInvoiceRenewable,
            RenewalPeriodInDays = tenantCreateRequest.RenewalPeriodInDays,
            IsTenancyActive = true
        };

        var tenancy = pmsDbContext.Tenancies.Add(tenancyObject);
        await pmsDbContext.SaveChangesAsync();

        var tenancyId = tenancy.Entity.Id;

        var invoiceObject = new Invoice
        {
            TenancyId = tenancyId,
            Discount = createInvoiceRequest.Discount,
            TenantId = tenantId,
            DateStart = createInvoiceRequest.DateStart,
            DateDue = createInvoiceRequest.DateDue,
            InvoiceStatus = createInvoiceRequest.InvoiceStatus,
        };

        // create invoice and return invoice id
        var invoice = pmsDbContext
            .Invoices
            .Add(invoiceObject);

        await pmsDbContext.SaveChangesAsync();

        var invoiceId = invoice.Entity.Id;

        // create invoice item
        var invoiceItemObject = new InvoiceItem
        {
            Description = "New Tenancy Rent",
            Amount = createInvoiceRequest.RentAmount,
            InvoiceId = invoiceId
        };

        pmsDbContext.InvoiceItems.Add(invoiceItemObject);
        await pmsDbContext.SaveChangesAsync();

        // return invoice record
        var invoiceRecord = await invoice.Entity.AsInvoiceListingRecord(pmsDbContext);

        return invoiceRecord;
    }

    public async Task<PaginatedResult<InvoiceListingItem>> GetAllInvoicesListingItems(int pageSize, int pageNumber,
        DateTime invoiceCreateDateFrom, DateTime invoiceCreateDateTill)
    {
        var query = pmsDbContext.Invoices.Where(invoice =>
            invoice.CreatedAt >= invoiceCreateDateFrom && invoice.CreatedAt <= invoiceCreateDateTill);

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var invoices = await query.OrderBy(invoice => invoice.DateStart)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var invoiceListingItems = new List<InvoiceListingItem>();

        foreach (var invoice in invoices)
        {
            var invoiceRecord = await invoice.AsInvoiceListingRecord(pmsDbContext);

            var tenancy = await pmsDbContext.Tenancies.FirstOrDefaultAsync(t => t.Id == invoice.TenancyId);

            var propertyListing =
                await pmsDbContext.PropertyListings.FirstOrDefaultAsync(p => p.Id == tenancy!.PropertyListingId);

            var tenant = await pmsDbContext.Tenants.FirstOrDefaultAsync(t => t.Id == invoice.TenantId);

            invoiceListingItems.Add(new InvoiceListingItem
            {
                Id = invoice.Id,
                TenantName = tenant?.Name ?? "Unknown",
                PropertyUnitName = propertyListing?.PropertyName ?? "Unknown",
                SubUnit = null, // todo: map Sub Unit when implementation is ready
                InvoiceStatus = invoice.InvoiceStatus,
                Amount = await invoice.TotalAmountMinusDiscount(pmsDbContext),
                IssuedDate = invoice.DateStart,
                DueDate = invoice.DateDue
            });
        }

        return new PaginatedResult<InvoiceListingItem>
        {
            Items = invoiceListingItems,
            TotalItems = totalItems,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    public async Task<PaginatedResult<MonetaryAccount>> GetAllMonetaryAccountsListingItems(int pageSize, int pageNumber)
    {
        var query = pmsDbContext.MonetaryAccounts;

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var monetaryAccounts = await query.OrderBy(account => account.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResult<MonetaryAccount>
        {
            Items = monetaryAccounts,
            TotalItems = totalItems,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    public async Task CreateMainMonetaryAccount()
    {
        // check if main account exists
        var mainAccountExists = await pmsDbContext.MonetaryAccounts.AnyAsync(account => account.AccountName == "Main");

        if (!mainAccountExists)
        {
            var mainAccount = new MonetaryAccount { AccountName = "Main" };

            pmsDbContext.MonetaryAccounts.Add(mainAccount);
            await pmsDbContext.SaveChangesAsync();
        }
    }

    public async Task UpdateInvoiceStatus(Guid invoiceId)
    {
        var invoice = await pmsDbContext
            .Invoices
            .Where(invoice => invoice.Id == invoiceId)
            .FirstOrDefaultAsync();

        if (invoice == null)
        {
            return;
        }

        var rentPaymentsSum = await pmsDbContext.RentPayments
            .Where(rentPayment => rentPayment.InvoiceId == invoice.Id)
            .SumAsync(rentPayment => rentPayment.Amount);

        var invoiceTotalAmount = await invoice.TotalAmountMinusDiscount(pmsDbContext);

        if (rentPaymentsSum - Tolerance > invoiceTotalAmount)
        {
            throw new InvalidOperationException("Invoice already been completely paid.");
        }

        invoice.InvoiceStatus = Math.Abs(rentPaymentsSum - invoiceTotalAmount) < Tolerance
            ? Invoice.InvoiceStatuses.Paid
            : Invoice.InvoiceStatuses.Outstanding;

        await pmsDbContext.SaveChangesAsync();
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

            await UpdateInvoiceStatus(request.InvoiceId);

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

    public async Task<PaginatedResult<TransactionListingRecord>> GetTransactionsListingsAsync(int pageNumber, int pageSize)
    {
        var query = pmsDbContext.Transactions;
        var transactions = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

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

    public async Task<InvoiceRecord> CreateInvoiceRecord(CreateInvoiceRequest createInvoiceRequest)
    {
        var invoiceObject = new Invoice
        {
            TenancyId = createInvoiceRequest.TenancyId,
            Discount = createInvoiceRequest.Discount,
            TenantId = createInvoiceRequest.TenantId,
            DateStart = DateTime.UtcNow,
            DateDue = createInvoiceRequest.DateDue,
            InvoiceStatus = Invoice.InvoiceStatuses.Pending
        };

        var invoice = pmsDbContext
            .Invoices
            .Add(invoiceObject);
        await pmsDbContext.SaveChangesAsync();
        var invoiceId = invoice.Entity.Id;

        foreach (var item in createInvoiceRequest.InvoiceItems)
        {
            var invoiceItemObject = new InvoiceItem
            {
                Description = item.Description,
                Amount = item.Amount,
                InvoiceId = invoiceId
            };
            pmsDbContext.InvoiceItems.Add(invoiceItemObject);
        }

        await pmsDbContext.SaveChangesAsync();

        var invoiceRecord = await invoice.Entity.AsInvoiceListingRecord(pmsDbContext);

        return invoiceRecord;

    }

    public async Task RenewInvoiceScheduledJob()
    {
        logger.LogInformation("RenewInvoiceScheduledJob was called at {Time}", DateTime.UtcNow);
        var tenancies = await pmsDbContext
            .Tenancies
            .Where(x => x.IsRenewable)
            .ToListAsync();

        foreach (var tenancy in tenancies)
        {
            var invoice = await pmsDbContext
                .Invoices
                .Where(x => x.TenancyId == tenancy.Id)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (invoice == null) continue;

            DateTime nextDateStart;
            int renewalDays = (int)tenancy.RenewalPeriodInDays!;

            // Calculate next renewal date
            if (renewalDays == 30)
            {
                // Same day next month (handles end-of-month automatically)
                nextDateStart = invoice.DateStart.AddMonths(1);
            }
            else if (invoice.DateStart.Day == 1)
            {
                // First-of-month logic
                int daysInMonth = DateTime.DaysInMonth(invoice.DateStart.Year, invoice.DateStart.Month);
                nextDateStart = renewalDays == daysInMonth
                    ? invoice.DateStart.AddMonths(1)
                    : invoice.DateStart.AddDays(renewalDays);
            }
            else
            {
                // Regular daily renewal
                nextDateStart = invoice.DateStart.AddDays(renewalDays);
            }

            // Ensure UTC datetime
            nextDateStart = DateTime.SpecifyKind(nextDateStart, DateTimeKind.Utc);

            if (DateTime.UtcNow >= nextDateStart)
            {
                // Create new invoice
                var invoiceItems = await pmsDbContext
                    .InvoiceItems
                    .Where(x => x.InvoiceId == invoice.Id)
                    .ToListAsync();

                var newInvoice = new Invoice
                {
                    TenancyId = tenancy.Id,
                    TenantId = tenancy.TenantId,
                    DateStart = nextDateStart,
                    InvoiceStatus = Invoice.InvoiceStatuses.Pending,
                    Notes = invoice.Notes,
                    CreatedAt = DateTime.UtcNow,
                };

                pmsDbContext.Invoices.Add(newInvoice);
                await pmsDbContext.SaveChangesAsync();  // Save to get new invoice ID

                // Copy invoice items
                foreach (var invoiceItem in invoiceItems)
                {
                    pmsDbContext.InvoiceItems.Add(new InvoiceItem
                    {
                        Description = invoiceItem.Description,
                        Amount = invoiceItem.Amount,
                        InvoiceId = newInvoice.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                await pmsDbContext.SaveChangesAsync();
            }
        }

        logger.LogInformation("Processed {Count} tenancies", tenancies.Count);
    }

    public async Task<PropertyListing?> GetPropertyListingFromRentPayment(Guid rentPaymentId)
    {

        var rentPayment = await pmsDbContext
            .RentPayments
            .Where(x => x.Id == rentPaymentId)
            .FirstOrDefaultAsync();

        if (rentPayment == null)
        {
            return null;
        }

        var invoice = await pmsDbContext
        .Invoices
        .Where(x => x.Id == rentPayment.InvoiceId)
        .FirstOrDefaultAsync();

        if (invoice == null)
        {
            return null;
        }

        var tenancy = await pmsDbContext
            .Tenancies
            .Where(x => x.Id == invoice.TenancyId)
            .FirstOrDefaultAsync();

        if (tenancy == null)
        {
            return null;
        }

        var propertyListing = await pmsDbContext
        .PropertyListings
        .Where(x => x.Id == tenancy.PropertyListingId)
        .FirstOrDefaultAsync();

        if (propertyListing == null)
        {
            return null;
        }

        return propertyListing;
    }

    public async Task<Tenant?> GetTenantFromRentPayment(Guid rentPaymentId)
    {
        var rentPayment = await pmsDbContext
            .RentPayments
            .Where(x => x.Id == rentPaymentId)
            .FirstOrDefaultAsync();

        if (rentPayment == null)
        {
            return null;
        }

        var tenant = await pmsDbContext
            .Tenants
            .Where(x => x.Id == rentPayment.TenantId)
            .FirstOrDefaultAsync();

        if (tenant == null)
        {
            return null;
        }

        return tenant;
    }

    public async Task<PropertyListing?> GetPropertyLisitngFromExpense(Guid expenseId)
    {

        var expense = await pmsDbContext
            .Expenses
            .Where(x => x.Id == expenseId)
            .FirstOrDefaultAsync();

        if (expense == null)
        {
            return null;
        }

        var propertyListing = await pmsDbContext
            .PropertyListings
            .Where(x => x.Id == expense.PropertyListingId)
            .FirstOrDefaultAsync();

        if (propertyListing == null)
        {
            return null;
        }

        return propertyListing;
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
            var invoiceTotalAmount = await invoice.TotalAmountMinusDiscount(pmsDbContext);
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
            totalOutstanding += await invoice.TotalAmountRemaining(pmsDbContext);
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
            totalOutstanding += await invoice.TotalAmountRemaining(pmsDbContext);
        }

        return totalOutstanding;
    }
}
