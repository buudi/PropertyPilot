using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.Extensions;
using PropertyPilot.Services.Generics;
using PropertyPilot.Services.InvoiceServices.Models;
using PropertyPilot.Services.TenantServices.Models;

namespace PropertyPilot.Services.FinanceServices;

public class FinancesService(PmsDbContext pmsDbContext)
{
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
}
