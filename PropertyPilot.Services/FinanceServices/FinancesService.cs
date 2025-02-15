using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.Extensions;
using PropertyPilot.Services.FinanceServices.Models;
using PropertyPilot.Services.Generics;
using PropertyPilot.Services.InvoiceServices.Models;
using PropertyPilot.Services.TenantServices.Models;

namespace PropertyPilot.Services.FinanceServices;

public class FinancesService(PmsDbContext pmsDbContext)
{
    private const double Tolerance = 1.0;
    private readonly Guid _mainMonetaryAccountGuid = Guid.Parse("7e174c5d-3756-4f9d-87b3-8f5e59f7f69e");
    private readonly Guid _stripeMonetaryAccountGuid = Guid.Parse("d24bde15-7ab2-46e9-9852-d99b51bc5e19");

    private async Task<bool> IsInvoicePaid(Invoice invoice)
    {
        var rentPaymentsSum = await pmsDbContext.RentPayments
            .Where(rentPayment => rentPayment.InvoiceId == invoice.Id)
            .SumAsync(rentPayment => rentPayment.Amount);

        var invoiceTotalAmount = await invoice.TotalAmountMinusDiscount(pmsDbContext);

        return rentPaymentsSum - Tolerance > invoiceTotalAmount;
    }

    private async Task<MonetaryAccount> UpdateAccountBalance(Transaction transaction)
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
            if (sourceAccount.Balance < transaction.Amount - Tolerance) throw new InvalidOperationException("Insufficient balance in the source account.");

            sourceAccount.Balance -= transaction.Amount;
            //pmsDbContext.MonetaryAccounts.Update(sourceAccount);
        }

        if (destinationAccount != null)
        {
            destinationAccount.Balance += transaction.Amount;
            //pmsDbContext.MonetaryAccounts.Update(destinationAccount);
        }

        await pmsDbContext.SaveChangesAsync();

        return destinationAccount ?? sourceAccount!;
    }

    public async Task<Invoice?> GetInvoiceByIdAsync(Guid invoiceId)
    {
        var invoice = await pmsDbContext
            .Invoices
            .Where(x => x.Id == invoiceId)
            .FirstOrDefaultAsync();

        return invoice ?? null;
    }

    public async Task<InvoiceRecord> CreateTenancyAndInvoiceOnTenantCreate(Guid tenantId, TenantCreateRequest tenantCreateRequest, CreateInvoiceRequest createInvoiceRequest)
    {

        var tenancyObject = new Tenancy
        {
            TenantId = tenantId,
            PropertyListingId = tenantCreateRequest.PropertyUnitId,
            TenancyStart = createInvoiceRequest.DateStart,
            TenancyEnd = tenantCreateRequest.TenancyEnd,
            IsMonthlyRenewable = tenantCreateRequest.IsInvoiceRenewable,
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
            IsRenewable = createInvoiceRequest.IsRenewable,
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

            var totalAmount = invoiceRecord.InvoiceItems.Sum(item => item.Amount);

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
                Amount = totalAmount,
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

            await UpdateAccountBalance(transaction);
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
            var transactionListing = await TransactionListingRecord.CreateAsync(transaction, pmsDbContext);
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

    public async Task<ExpenseTransactionRecord> RecordExpenseAsync(CreateExpenseRequest createExpenseRequest)
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

        await UpdateAccountBalance(transaction);

        return await expense.AsExpenseTransactionRecord(pmsDbContext);
    }

}
