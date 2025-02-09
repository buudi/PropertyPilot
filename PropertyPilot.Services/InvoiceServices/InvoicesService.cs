using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.Extensions;
using PropertyPilot.Services.InvoiceServices.Models;

namespace PropertyPilot.Services.InvoiceServices;

public class InvoicesService(PmsDbContext pmsDbContext)
{
    public async Task<InvoiceRecord> CreateInvoiceOnTenantCreate(Guid tenantId, CreateInvoiceRequest createInvoiceRequest)
    {
        var invoiceObject = new Invoice
        {
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

    //public async Task<List<InvoiceListingItem>> GetAllInvoicesListingItems()
    //{
    //}
}
