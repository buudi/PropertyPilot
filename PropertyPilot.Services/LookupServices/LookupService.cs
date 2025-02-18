using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.Extensions;
using PropertyPilot.Services.LookupServices.Models;

namespace PropertyPilot.Services.LookupServices;

public class LookupService(PmsDbContext pmsDbContext)
{
    public async Task<List<PropertyListingsLookup>> PropertyListingsLookup()
    {
        var listings = await pmsDbContext.PropertyListings.ToListAsync();

        var lookupValues = new List<PropertyListingsLookup>();
        foreach (var property in listings)
        {
            var lookup = new PropertyListingsLookup
            {
                Id = property.Id,
                PropertyName = property.PropertyName,
                PropertyType = property.PropertyType,
                UnitsCount = property.UnitsCount,
            };
            lookupValues.Add(lookup);
        }

        return lookupValues;
    }

    public async Task<List<MonetaryAccountLookup>> MonetaryAccountLookup()
    {
        var accounts = await pmsDbContext.MonetaryAccounts.ToListAsync();

        var lookupValues = new List<MonetaryAccountLookup>();
        foreach (var account in accounts)
        {
            var lookup = new MonetaryAccountLookup
            {
                Id = account.Id,
                AccountName = account.AccountName,
                Balance = account.Balance
            };
            lookupValues.Add(lookup);
        }

        return lookupValues;
    }


    public async Task<List<TenantLookup>> TenantLookup()
    {
        var tenants = await pmsDbContext.Tenants.ToListAsync();

        var lookupValues = new List<TenantLookup>();

        foreach (var tenant in tenants)
        {
            var lookup = new TenantLookup
            {
                Id = tenant.Id,
                Name = tenant.Name,
            };
            lookupValues.Add(lookup);
        }

        return lookupValues;
    }

    public async Task<List<InvoiceLookup>> InvoiceLookupForTenant(Guid tenantId)
    {
        var invoices = await pmsDbContext.Invoices
            .Where(x => x.TenantId == tenantId)
            .Where(x => x.InvoiceStatus == Invoice.InvoiceStatuses.Pending ||
                        x.InvoiceStatus == Invoice.InvoiceStatuses.Outstanding)
            .ToListAsync();

        var invoiceLookups = new List<InvoiceLookup>();
        foreach (var invoice in invoices)
        {

            var amountRemaining = await invoice.TotalAmountRemaining(pmsDbContext);

            var lookup = new InvoiceLookup
            {
                Id = invoice.Id,
                DateStart = invoice.DateStart,
                InvoiceStatus = invoice.InvoiceStatus,
                AmountRemaining = amountRemaining
            };

            invoiceLookups.Add(lookup);
        }

        return invoiceLookups;
    }

}
