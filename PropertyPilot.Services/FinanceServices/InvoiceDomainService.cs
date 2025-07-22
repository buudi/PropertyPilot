using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;

namespace PropertyPilot.Services.FinanceServices;

public class InvoiceDomainService
{
    private readonly PmsDbContext _db;
    public InvoiceDomainService(PmsDbContext db)
    {
        _db = db;
    }

    public async Task<double> GetTotalAmountMinusDiscountAsync(Invoice invoice)
    {
        var invoiceItems = await _db.InvoiceItems.Where(item => item.InvoiceId == invoice.Id).ToListAsync();
        var totalAmount = invoiceItems.Sum(item => item.Amount);
        if (invoice.Discount.HasValue) totalAmount -= invoice.Discount.Value;
        return totalAmount;
    }

    public async Task<double> GetTotalAmountRemainingAsync(Invoice invoice)
    {
        var amount = await GetTotalAmountMinusDiscountAsync(invoice);
        var sumPaid = await _db.RentPayments
            .Where(x => x.InvoiceId == invoice.Id)
            .SumAsync(x => x.Amount);
        var amountRemaining = amount - sumPaid;
        return amountRemaining;
    }
} 