using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Services.CaretakerPortalServices;
using PropertyPilot.Services.CaretakerPortalServices.Models.Properties;
using PropertyPilot.Services.CaretakerPortalServices.Models.Properties.TenantPage;
using PropertyPilot.Services.Extensions;
using PropertyPilot.Services.FinanceServices;
using PropertyPilot.Services.Generics;
using PropertyPilot.Services.TenantPortalServices.Models.Settings;

namespace PropertyPilot.Services.TenantPortalServices;

public class TenantPortalService(PmsDbContext pmsDbContext, CaretakerPortalService caretakerPortalService, FinancesService financesService)
{
    public async Task<BasicTenantInfo?> GetBasicTenantInfo(Guid tenantAccountId)
    {
        var tenantAccount = await pmsDbContext
            .TenantAccounts
            .Where(x => x.Id == tenantAccountId)
            .FirstOrDefaultAsync();

        if (tenantAccount == null)
            return null;

        var tenantAccountRecord = await tenantAccount.GetTenantAccountRecord(pmsDbContext);

        return new BasicTenantInfo
        {
            Email = tenantAccountRecord.TenantAccount.Email,
            Name = tenantAccountRecord.Tenant.Name
        };
    }

    public async Task<TenancyInformation?> GetCurrentActiveTenancyInfo(Guid tenantAccountId)
    {
        var tenantAccount = await pmsDbContext.TenantAccounts.FirstOrDefaultAsync(x => x.Id == tenantAccountId);
        if (tenantAccount == null || tenantAccount.TenantId == null)
            return null;

        var tenantAccountRecord = await tenantAccount.GetTenantAccountRecord(pmsDbContext);

        var activeTenancy = await pmsDbContext.Tenancies
            .Where(x => x.TenantId == tenantAccountRecord.Tenant.Id && x.IsTenancyActive)
            .OrderByDescending(x => x.TenancyStart)
            .FirstOrDefaultAsync();

        if (activeTenancy == null)
            return null;

        return await caretakerPortalService.GetTenancyInformation(activeTenancy.Id);
    }

    public async Task<double> GetOutstandingAmount(Guid tenantAccountId)
    {
        var tenantId = await pmsDbContext.TenantAccounts
            .Where(x => x.Id == tenantAccountId)
            .Select(x => x.TenantId)
            .FirstOrDefaultAsync();

        var tenantOutstanding = await financesService.IsTenantOutstanding((Guid)tenantId);

        return tenantOutstanding.OutstandingAmount;
    }

    public async Task<PaginatedResult<PaymentsTabListing>> GetAllPaymentsForTenantAsync(Guid tenantAccountId, int pageSize, int pageNumber)
    {
        var tenantAccount = await pmsDbContext.TenantAccounts.FirstOrDefaultAsync(x => x.Id == tenantAccountId);
        if (tenantAccount == null || tenantAccount.TenantId == null)
        {
            return new PaginatedResult<PaymentsTabListing>
            {
                Items = new List<PaymentsTabListing>(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = 0,
                TotalPages = 0
            };
        }

        var tenantId = tenantAccount.TenantId.Value;

        var totalItems = await pmsDbContext.RentPayments
            .Where(x => x.TenantId == tenantId)
            .CountAsync();

        var rentPayments = await pmsDbContext.RentPayments
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var paymentsTabListings = new List<PaymentsTabListing>();
        foreach (var rentPayment in rentPayments)
        {
            var subUnitName = "";
            var tenancy = await pmsDbContext.Tenancies
                .Where(t => t.TenantId == tenantId && t.IsTenancyActive)
                .FirstOrDefaultAsync();

            if (tenancy != null && tenancy.SubUnitId.HasValue)
            {
                var subUnit = await pmsDbContext.SubUnits
                    .Where(su => su.Id == tenancy.SubUnitId.Value)
                    .FirstOrDefaultAsync();
                subUnitName = subUnit?.IdentifierName ?? "";
            }

            var paymentsTabListing = new PaymentsTabListing
            {
                PaymentId = rentPayment.Id,
                TenantName = tenantAccount.Email,
                SubUnitName = subUnitName,
                PaymentDate = rentPayment.CreatedAt,
                PaymentMethod = rentPayment.PaymentMethod,
                PaymentAmount = rentPayment.Amount
            };

            paymentsTabListings.Add(paymentsTabListing);
        }

        return new PaginatedResult<PaymentsTabListing>
        {
            Items = paymentsTabListings,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<PaginatedResult<InvoicesTabListing>> GetAllInvoicesForTenantAsync(Guid tenantAccountId, int pageSize, int pageNumber)
    {
        var tenantAccount = await pmsDbContext.TenantAccounts.FirstOrDefaultAsync(x => x.Id == tenantAccountId);
        if (tenantAccount == null || tenantAccount.TenantId == null)
        {
            return new PaginatedResult<InvoicesTabListing>
            {
                Items = new List<InvoicesTabListing>(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = 0,
                TotalPages = 0
            };
        }

        var tenantId = tenantAccount.TenantId.Value;

        var totalItems = await pmsDbContext.Invoices
            .Where(x => x.TenantId == tenantId)
            .CountAsync();

        var invoices = await pmsDbContext.Invoices
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var invoicesTabListings = new List<InvoicesTabListing>();
        foreach (var invoice in invoices)
        {
            var tenancy = await pmsDbContext.Tenancies
                .Where(t => t.Id == invoice.TenancyId)
                .FirstOrDefaultAsync();

            var subUnitName = "";
            if (tenancy != null && tenancy.SubUnitId.HasValue)
            {
                var subUnit = await pmsDbContext.SubUnits
                    .Where(su => su.Id == tenancy.SubUnitId.Value)
                    .FirstOrDefaultAsync();
                subUnitName = subUnit?.IdentifierName ?? "";
            }

            var invoicesTabListing = new InvoicesTabListing
            {
                InvoiceId = invoice.Id,
                TenantName = tenantAccount.Email,
                SubUnitName = subUnitName,
                IssuedDate = invoice.CreatedAt,
                IsAutoGenerated = true, // Placeholder, adjust as needed
                InvoiceStatus = invoice.InvoiceStatus,
                Amount = 0, // Set actual amount if available
                AmountDue = 0 // Set actual amount due if available
            };

            invoicesTabListings.Add(invoicesTabListing);
        }

        return new PaginatedResult<InvoicesTabListing>
        {
            Items = invoicesTabListings,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }
}
