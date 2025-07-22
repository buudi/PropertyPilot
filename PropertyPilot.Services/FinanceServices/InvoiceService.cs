using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.FinanceServices.Models;
using PropertyPilot.Services.InvoiceServices.Models;
using PropertyPilot.Services.TenantServices.Models;

namespace PropertyPilot.Services.FinanceServices;

public class InvoiceService
{
    private readonly PmsDbContext _db;
    private readonly ILogger<InvoiceService> _logger;
    private const double Tolerance = 1.0;

    public InvoiceService(PmsDbContext db, ILogger<InvoiceService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Invoice?> GetInvoiceByIdAsync(Guid invoiceId)
    {
        var invoice = await _db.Invoices.Where(x => x.Id == invoiceId).FirstOrDefaultAsync();
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
        var tenancy = _db.Tenancies.Add(tenancyObject);
        await _db.SaveChangesAsync();
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
        var invoice = _db.Invoices.Add(invoiceObject);
        await _db.SaveChangesAsync();
        var invoiceId = invoice.Entity.Id;
        var invoiceItemObject = new InvoiceItem
        {
            Description = "New Tenancy Rent",
            Amount = createInvoiceRequest.RentAmount,
            InvoiceId = invoiceId
        };
        _db.InvoiceItems.Add(invoiceItemObject);
        await _db.SaveChangesAsync();
        var invoiceRecord = await invoice.Entity.AsInvoiceListingRecord(_db);
        return invoiceRecord;
    }

    public async Task<PaginatedResult<InvoiceListingItem>> GetAllInvoicesListingItems(int pageSize, int pageNumber, DateTime invoiceCreateDateFrom, DateTime invoiceCreateDateTill)
    {
        var query = _db.Invoices.Where(invoice => invoice.CreatedAt >= invoiceCreateDateFrom && invoice.CreatedAt <= invoiceCreateDateTill);
        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        var invoices = await query.OrderBy(invoice => invoice.DateStart).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        var invoiceListingItems = new List<InvoiceListingItem>();
        foreach (var invoice in invoices)
        {
            var invoiceRecord = await invoice.AsInvoiceListingRecord(_db);
            var tenancy = await _db.Tenancies.FirstOrDefaultAsync(t => t.Id == invoice.TenancyId);
            var propertyListing = await _db.PropertyListings.FirstOrDefaultAsync(p => p.Id == tenancy!.PropertyListingId);
            var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == invoice.TenantId);
            invoiceListingItems.Add(new InvoiceListingItem
            {
                Id = invoice.Id,
                TenantName = tenant?.Name ?? "Unknown",
                PropertyUnitName = propertyListing?.PropertyName ?? "Unknown",
                SubUnit = null, // todo: map Sub Unit when implementation is ready
                InvoiceStatus = invoice.InvoiceStatus,
                Amount = await invoice.TotalAmountMinusDiscount(_db),
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

    public async Task CreateMainMonetaryAccount()
    {
        var mainAccountExists = await _db.MonetaryAccounts.AnyAsync(account => account.AccountName == "Main");
        if (!mainAccountExists)
        {
            var mainAccount = new MonetaryAccount { AccountName = "Main" };
            _db.MonetaryAccounts.Add(mainAccount);
            await _db.SaveChangesAsync();
        }
    }

    public async Task UpdateInvoiceStatus(Guid invoiceId)
    {
        var invoice = await _db.Invoices.Where(invoice => invoice.Id == invoiceId).FirstOrDefaultAsync();
        if (invoice == null)
        {
            return;
        }
        var rentPaymentsSum = await _db.RentPayments.Where(rentPayment => rentPayment.InvoiceId == invoice.Id).SumAsync(rentPayment => rentPayment.Amount);
        var invoiceTotalAmount = await invoice.TotalAmountMinusDiscount(_db);
        if (rentPaymentsSum - Tolerance > invoiceTotalAmount)
        {
            throw new InvalidOperationException("Invoice already been completely paid.");
        }
        invoice.InvoiceStatus = Math.Abs(rentPaymentsSum - invoiceTotalAmount) < Tolerance ? Invoice.InvoiceStatuses.Paid : Invoice.InvoiceStatuses.Outstanding;
        await _db.SaveChangesAsync();
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
        var invoice = _db.Invoices.Add(invoiceObject);
        await _db.SaveChangesAsync();
        var invoiceId = invoice.Entity.Id;
        foreach (var item in createInvoiceRequest.InvoiceItems)
        {
            var invoiceItemObject = new InvoiceItem
            {
                Description = item.Description,
                Amount = item.Amount,
                InvoiceId = invoiceId
            };
            _db.InvoiceItems.Add(invoiceItemObject);
        }
        await _db.SaveChangesAsync();
        var invoiceRecord = await invoice.Entity.AsInvoiceListingRecord(_db);
        return invoiceRecord;
    }

    public async Task RenewInvoiceScheduledJob()
    {
        _logger.LogInformation("RenewInvoiceScheduledJob was called at {Time}", DateTime.UtcNow);
        var tenancies = await _db.Tenancies.Where(x => x.IsRenewable && x.RenewalPeriodInDays.HasValue).ToListAsync();
        foreach (var tenancy in tenancies)
        {
            var lastInvoice = await _db.Invoices.Where(x => x.TenancyId == tenancy.Id).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync();
            if (lastInvoice == null)
                continue;
            int renewalDays = tenancy.RenewalPeriodInDays!.Value;
            DateTime nextDateStart;
            if (renewalDays == 30)
            {
                nextDateStart = lastInvoice.DateStart.AddMonths(1);
            }
            else if (lastInvoice.DateStart.Day == 1)
            {
                int daysInMonth = DateTime.DaysInMonth(lastInvoice.DateStart.Year, lastInvoice.DateStart.Month);
                nextDateStart = renewalDays == daysInMonth ? lastInvoice.DateStart.AddMonths(1) : lastInvoice.DateStart.AddDays(renewalDays);
            }
            else
            {
                nextDateStart = lastInvoice.DateStart.AddDays(renewalDays);
            }
            nextDateStart = DateTime.SpecifyKind(nextDateStart, DateTimeKind.Utc);
            if (DateTime.UtcNow < nextDateStart)
                continue;
            var newInvoice = new Invoice
            {
                TenancyId = tenancy.Id,
                TenantId = tenancy.TenantId,
                DateStart = nextDateStart,
                InvoiceStatus = Invoice.InvoiceStatuses.Pending,
                Notes = "Auto-generated based on tenancy rent",
                CreatedAt = DateTime.UtcNow
            };
            _db.Invoices.Add(newInvoice);
            await _db.SaveChangesAsync();
            _db.InvoiceItems.Add(new InvoiceItem
            {
                Description = "Rent",
                Amount = tenancy.AssignedRent,
                InvoiceId = newInvoice.Id,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }
        _logger.LogInformation("Processed {Count} tenancies", tenancies.Count);
    }
} 