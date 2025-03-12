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

    public async Task<List<UserLookup>> UsersLookup()
    {
        var users = await pmsDbContext.PropertyPilotUsers
            .ToListAsync();

        var usersLookup = new List<UserLookup>();
        foreach (var user in users)
        {
            usersLookup.Add(new UserLookup { Id = user.Id, Name = user.Name });
        }

        return usersLookup;
    }

    public async Task<List<TenancyLookup>> TenancyLookups(Guid tenantId)
    {
        var tenancies = await pmsDbContext.Tenancies
            .Where(x => x.TenantId == tenantId)
            .ToListAsync();

        if (tenancies.Count == 0)
        {
            return [];
        }

        var lookups = new List<TenancyLookup>();
        foreach (var tenancy in tenancies)
        {
            var propertyName = await pmsDbContext.PropertyListings
                .Where(x => x.Id == tenancy.PropertyListingId)
                .Select(x => x.PropertyName)
                .FirstOrDefaultAsync();

            var subUnitIdentifierName = await pmsDbContext.SubUnits
                .Where(x => x.Id == tenancy.SubUnitId)
                .Select(x => x.IdentifierName)
                .FirstOrDefaultAsync();

            var lookup = new TenancyLookup
            {
                Id = tenancy.Id,
                PropertyListingName = propertyName!,
                SubUnitIdentifierName = subUnitIdentifierName,
                TenancyStart = tenancy.TenancyStart,
                TenancyEnd = tenancy.TenancyEnd,
                IsMonthlyRenewable = tenancy.IsMonthlyRenewable,
                IsTenancyActive = tenancy.IsTenancyActive,
                EvacuationDate = tenancy.EvacuationDate
            };

            lookups.Add(lookup);
        }

        return lookups;
    }

    public async Task<List<SubUnitsLookup>> SubUnitsLookup(Guid propertyListingId)
    {
        var subUnits = await pmsDbContext.SubUnits
            .Where(x => x.PropertyListingId == propertyListingId)
            .ToListAsync();

        if (subUnits.Count == 0)
        {
            return new List<SubUnitsLookup>();
        }

        var lookUps = new List<SubUnitsLookup>();
        foreach (var subUnit in subUnits)
        {
            var lookup = new SubUnitsLookup
            {
                Id = subUnit.Id,
                IdentifierName = subUnit.IdentifierName
            };
            lookUps.Add(lookup);
        }

        return lookUps;
    }
}
