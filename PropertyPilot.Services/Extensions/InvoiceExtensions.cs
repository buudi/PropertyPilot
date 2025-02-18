using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.InvoiceServices.Models;

namespace PropertyPilot.Services.Extensions;

public static class InvoiceExtensions
{
    public static async Task<InvoiceRecord> AsInvoiceListingRecord(this Invoice invoice, PmsDbContext pmsDbContext)
    {
        var invoiceItems = await pmsDbContext
            .InvoiceItems
            .Where(x => x.InvoiceId == invoice.Id)
            .ToListAsync();

        return new InvoiceRecord
        {
            Invoice = invoice,
            InvoiceItems = invoiceItems
        };
    }

    public static async Task<double> TotalAmountMinusDiscount(this Invoice invoice, PmsDbContext pmsDbContext)
    {
        var invoiceItems = await pmsDbContext.InvoiceItems.Where(item => item.InvoiceId == invoice.Id).ToListAsync();

        var totalAmount = invoiceItems.Sum(item => item.Amount);

        if (invoice.Discount.HasValue) totalAmount -= invoice.Discount.Value;

        return totalAmount;
    }

    public static async Task<double> TotalAmountRemaining(this Invoice invoice, PmsDbContext pmsDbContext)
    {
        var amount = await invoice.TotalAmountMinusDiscount(pmsDbContext);

        // check rent payments where invoice is invoice.id and sum the Amounts
        var sumPaid = await pmsDbContext.RentPayments
            .Where(x => x.InvoiceId == invoice.Id)
            .SumAsync(x => x.Amount);

        var amountRemaining = amount - sumPaid;
        return amountRemaining;
    }

}
