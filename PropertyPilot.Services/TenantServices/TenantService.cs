using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.Extensions;
using PropertyPilot.Services.FinanceServices;
using PropertyPilot.Services.Generics;
using PropertyPilot.Services.InvoiceServices.Models;
using PropertyPilot.Services.TenantServices.Models;

namespace PropertyPilot.Services.TenantServices;

public class TenantService(PmsDbContext pmsDbContext, FinancesService invoicesService)
{
    public async Task<PaginatedResult<TenantListingRecord>> GetAllTenantsListingAsync(int pageNumber, int pageSize)
    {

        var totalTenants = await pmsDbContext.Tenants.CountAsync();

        var tenants = await pmsDbContext.Tenants
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var tenantsListing = new List<TenantListingRecord>();

        foreach (var t in tenants)
        {
            tenantsListing.Add(await t.AsTenantListingRecord(pmsDbContext));
        }

        return new PaginatedResult<TenantListingRecord>
        {
            Items = tenantsListing,
            TotalItems = totalTenants,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalTenants / (double)pageSize)
        };
    }

    public async Task<TenantListingRecord?> GetTenantRecordAsync(Guid tenantId)
    {
        var tenant = await pmsDbContext.Tenants
            .Where(x => x.Id == tenantId)
            .FirstOrDefaultAsync();

        if (tenant == null)
            return null;

        return await tenant.AsTenantListingRecord(pmsDbContext);
    }

    public async Task<TenantListingRecord> CreateTenantAsync(TenantCreateRequest tenantCreateRequest)
    {
        var newTenant = new Tenant
        {
            Name = tenantCreateRequest.Name,
            PhoneNumber = tenantCreateRequest.PhoneNumber,
            Email = tenantCreateRequest.Email,
            TenantIdentification = tenantCreateRequest.TenantIdentification
        };

        var tenant = pmsDbContext.Tenants.Add(newTenant);
        await pmsDbContext.SaveChangesAsync();

        var tenantId = tenant.Entity.Id;

        // create invoice for newly created tenant
        var createInvoice = new CreateInvoiceOnNewTenantRequest
        {
            RentAmount = tenantCreateRequest.AssignedRent,
            Discount = tenantCreateRequest.OneTimeDiscount,
            DateStart = tenantCreateRequest.TenancyStart,
            IsRenewable = tenantCreateRequest.IsInvoiceRenewable,
            InvoiceStatus = Invoice.InvoiceStatuses.Pending
        };

        await invoicesService.CreateTenancyAndInvoiceOnTenantCreate(tenantId, tenantCreateRequest, createInvoice);

        return await newTenant.AsTenantListingRecord(pmsDbContext);
    }

    public async Task<List<TenantListingRecord>> PopulateTestTenantData(Guid propertyUnitId, DateTime dateFrom, DateTime dateTill)
    {
        var subUnits = await pmsDbContext.SubUnits
            .Where(x => x.PropertyListingId == propertyUnitId)
            .ToListAsync();

        var propertyName = await pmsDbContext.PropertyListings
            .Where(x => x.Id == propertyUnitId)
            .Select(x => x.PropertyName)
            .FirstOrDefaultAsync();

        var tenantCreateRequests = new List<TenantCreateRequest>();

        foreach (var subUnit in subUnits)
        {
            Random random = new Random();
            int randomRent = random.Next(10, 31) * 100;
            int randomTenantNumber = random.Next(19, 9999);

            var tenantCreateRequest = new TenantCreateRequest
            {
                Name = $"tenant populate {randomTenantNumber}",
                PhoneNumber = "0110-123-123",
                Email = $"tenant.{subUnit.IdentifierName}@{propertyName}.test",
                TenantIdentification = "784-123456789-090",
                IsAccountActive = true,
                IsInvoiceRenewable = false,
                PropertyUnitId = propertyUnitId,
                SubUnitId = subUnit.Id,
                AssignedRent = randomRent,
                TenancyStart = dateFrom,
                TenancyEnd = dateTill,
            };

            tenantCreateRequests.Add(tenantCreateRequest);
        }

        var createdTenantListingRecords = new List<TenantListingRecord>();
        foreach (var request in tenantCreateRequests)
        {
            var record = await CreateTenantAsync(request);
            createdTenantListingRecords.Add(record);
        }

        return createdTenantListingRecords;
    }

    public async Task<int> ThisMonthEvacuatingTenantsCount(Guid propertyId)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var startOfNextMonth = startOfMonth.AddMonths(1);

        var count = await pmsDbContext.Tenancies
            .Where(t => t.PropertyListingId == propertyId &&
                        t.IsTenancyActive &&
                        t.EvacuationDate != null &&
                        t.EvacuationDate >= startOfMonth &&
                        t.EvacuationDate < startOfNextMonth)
            .CountAsync();

        return count;
    }

}
