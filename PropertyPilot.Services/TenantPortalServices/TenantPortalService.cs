using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Services.CaretakerPortalServices;
using PropertyPilot.Services.CaretakerPortalServices.Models.Properties;
using PropertyPilot.Services.CaretakerPortalServices.Models.Properties.TenantPage;
using PropertyPilot.Services.Extensions;
using PropertyPilot.Services.FinanceServices;
using PropertyPilot.Services.Generics;
using PropertyPilot.Services.InvoiceServices.Models;
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

        var totalItems = await pmsDbContext
            .Invoices
            .Where(x => x.TenantId == tenantId)
            .CountAsync();

        var invoices = await pmsDbContext
            .Invoices
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var invoiceRecords = new List<InvoiceRecord>();
        foreach (var invoice in invoices)
        {
            var record = await invoice.AsInvoiceListingRecord(pmsDbContext);
            invoiceRecords.Add(record);
        }

        var invoicesTabListings = new List<InvoicesTabListing>();

        foreach (var invoiceRecord in invoiceRecords)
        {
            var tenant = await pmsDbContext
                .Tenants
                .Where(x => x.Id == invoiceRecord.Invoice.TenantId)
                .FirstOrDefaultAsync();

            var tenancy = await pmsDbContext
                .Tenancies
                .Where(x => x.Id == invoiceRecord.Invoice.TenancyId)
                .FirstOrDefaultAsync();

            var subUnitName = await pmsDbContext
                .SubUnits
                .Where(x => x.Id == tenancy!.SubUnitId)
                .Select(x => x.IdentifierName)
                .FirstOrDefaultAsync();

            // todo: handle manual and auto invoice generated flag
            var autoGenerated = true; // temporary place holder

            var totalAmount = await invoiceRecord.Invoice.TotalAmountMinusDiscount(pmsDbContext);

            var amountDue = await invoiceRecord.Invoice.TotalAmountRemaining(pmsDbContext);

            var invoiceTabListing = new InvoicesTabListing()
            {
                InvoiceId = invoiceRecord.Invoice.Id,
                TenantName = tenant!.Name,
                SubUnitName = subUnitName ?? "",
                IssuedDate = invoiceRecord.Invoice.CreatedAt,
                IsAutoGenerated = autoGenerated,
                InvoiceStatus = invoiceRecord.Invoice.InvoiceStatus,
                Amount = totalAmount,
                AmountDue = amountDue
            };

            invoicesTabListings.Add(invoiceTabListing);
        }

        return new PaginatedResult<InvoicesTabListing>
        {
            Items = invoicesTabListings,
            PageNumber = pageNumber,
            TotalItems = totalItems,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<CaretakerDetailsDto?> GetCaretakerDetailsForTenant(Guid tenantAccountId)
    {
        // Find the tenant account
        var tenantAccount = await pmsDbContext.TenantAccounts.FirstOrDefaultAsync(x => x.Id == tenantAccountId);
        if (tenantAccount == null || tenantAccount.TenantId == null)
            return null;

        // Find the active tenancy
        var activeTenancy = await pmsDbContext.Tenancies
            .Where(x => x.TenantId == tenantAccount.TenantId && x.IsTenancyActive)
            .OrderByDescending(x => x.TenancyStart)
            .FirstOrDefaultAsync();
        if (activeTenancy == null)
            return null;

        // Find the assigned caretaker for the property
        var assignedCaretaker = await pmsDbContext.AssignedCaretakerProperties
            .Where(x => x.PropertyListingId == activeTenancy.PropertyListingId)
            .FirstOrDefaultAsync();
        if (assignedCaretaker == null)
            return null;

        // Get caretaker user details
        var caretaker = await pmsDbContext.PropertyPilotUsers
            .Where(x => x.Id == assignedCaretaker.UserId && x.Role == PropertyPilot.Dal.Models.PropertyPilotUser.UserRoles.Caretaker)
            .FirstOrDefaultAsync();
        if (caretaker == null)
            return null;

        return new CaretakerDetailsDto
        {
            Id = caretaker.Id,
            Name = caretaker.Name,
            Email = caretaker.Email,
            Role = caretaker.Role
        };
    }

    public async Task<List<RecentActivityDto>> GetRecentActivityForTenant(Guid tenantAccountId, int limit = 10)
    {
        var tenantAccount = await pmsDbContext.TenantAccounts.FirstOrDefaultAsync(x => x.Id == tenantAccountId);
        if (tenantAccount == null || tenantAccount.TenantId == null)
            return new List<RecentActivityDto>();
        var tenantId = tenantAccount.TenantId.Value;

        var payments = await pmsDbContext.RentPayments
            .Where(x => x.TenantId == tenantId)
            .Select(x => new RecentActivityDto
            {
                Type = "payment",
                Date = x.CreatedAt,
                Title = "Rent Payment Received",
                Amount = (decimal)x.Amount,
                ReferenceId = x.Id
            })
            .ToListAsync();

        var invoiceEntities = await pmsDbContext.Invoices
        .Where(x => x.TenantId == tenantId)
        .ToListAsync();

        var invoices = new List<RecentActivityDto>();

        foreach (var x in invoiceEntities)
        {
            var amount = await x.TotalAmountMinusDiscount(pmsDbContext);
            invoices.Add(new RecentActivityDto
            {
                Type = "invoice",
                Date = x.CreatedAt,
                Title = "Invoice Issued",
                Amount = (decimal)amount,
                ReferenceId = x.Id
            });
        }


        var tenancies = await pmsDbContext.Tenancies
            .Where(x => x.TenantId == tenantId)
            .Select(x => new RecentActivityDto
            {
                Type = "lease",
                Date = x.TenancyStart,
                Title = "Lease Started",
                Description = "Lease started for property.",
                ReferenceId = x.Id
            })
            .ToListAsync();

        // Optionally add lease end events
        tenancies.AddRange(
            (await pmsDbContext.Tenancies
                .Where(x => x.TenantId == tenantId && x.TenancyEnd != null)
                .Select(x => new RecentActivityDto
                {
                    Type = "lease",
                    Date = x.TenancyEnd.Value,
                    Title = "Lease Ended",
                    Description = "Lease ended for property.",
                    ReferenceId = x.Id
                })
                .ToListAsync())
        );

        var all = payments.Concat(invoices).Concat(tenancies)
            .OrderByDescending(x => x.Date)
            .Take(limit)
            .ToList();
        return all;
    }

    public async Task<PaginatedResult<RecentActivityDto>> GetPaginatedActivityForTenant(Guid tenantAccountId, int pageSize, int pageNumber)
    {
        var tenantAccount = await pmsDbContext.TenantAccounts.FirstOrDefaultAsync(x => x.Id == tenantAccountId);
        if (tenantAccount == null || tenantAccount.TenantId == null)
        {
            return new PaginatedResult<RecentActivityDto>
            {
                Items = new List<RecentActivityDto>(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = 0,
                TotalPages = 0
            };
        }
        
        var tenantId = tenantAccount.TenantId.Value;

        var payments = await pmsDbContext.RentPayments
            .Where(x => x.TenantId == tenantId)
            .Select(x => new RecentActivityDto
            {
                Type = "payment",
                Date = x.CreatedAt,
                Title = "Rent Payment Received",
                Amount = (decimal)x.Amount,
                ReferenceId = x.Id
            })
            .ToListAsync();

        var invoiceEntities = await pmsDbContext.Invoices
            .Where(x => x.TenantId == tenantId)
            .ToListAsync();

        var invoices = new List<RecentActivityDto>();

        foreach (var x in invoiceEntities)
        {
            var amount = await x.TotalAmountMinusDiscount(pmsDbContext);
            invoices.Add(new RecentActivityDto
            {
                Type = "invoice",
                Date = x.CreatedAt,
                Title = "Invoice Issued",
                Amount = (decimal)amount,
                ReferenceId = x.Id
            });
        }

        var tenancies = await pmsDbContext.Tenancies
            .Where(x => x.TenantId == tenantId)
            .Select(x => new RecentActivityDto
            {
                Type = "lease",
                Date = x.TenancyStart,
                Title = "Lease Started",
                Description = "Lease started for property.",
                ReferenceId = x.Id
            })
            .ToListAsync();

        // Optionally add lease end events
        tenancies.AddRange(
            (await pmsDbContext.Tenancies
                .Where(x => x.TenantId == tenantId && x.TenancyEnd != null)
                .Select(x => new RecentActivityDto
                {
                    Type = "lease",
                    Date = x.TenancyEnd.Value,
                    Title = "Lease Ended",
                    Description = "Lease ended for property.",
                    ReferenceId = x.Id
                })
                .ToListAsync())
        );

        var all = payments.Concat(invoices).Concat(tenancies)
            .OrderByDescending(x => x.Date)
            .ToList();

        var totalItems = all.Count;
        var paginatedItems = all
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedResult<RecentActivityDto>
        {
            Items = paginatedItems,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<TenantSettingsProfile?> GetTenantProfile(Guid tenantAccountId)
    {
        var tenantAccount = await pmsDbContext
            .TenantAccounts
            .Where(x => x.Id == tenantAccountId)
            .FirstOrDefaultAsync();

        if (tenantAccount == null || tenantAccount.TenantId == null)
            return null;

        var tenant = await pmsDbContext.Tenants
            .Where(x => x.Id == tenantAccount.TenantId)
            .FirstOrDefaultAsync();

        if (tenant == null)
            return null;

        return new TenantSettingsProfile
        {
            Name = tenant.Name,
            PhoneNumber = tenant.PhoneNumber,
            Email = tenant.Email,
            TenantIdentification = tenant.TenantIdentification
        };
    }

    public async Task EditTenantProfile(Guid tenantAccountId, EditTenantProfile editTenantProfile)
    {
        var tenantAccount = await pmsDbContext
            .TenantAccounts
            .Where(x => x.Id == tenantAccountId)
            .FirstOrDefaultAsync();

        if (tenantAccount == null || tenantAccount.TenantId == null)
            throw new InvalidOperationException("Tenant account not found");

        var tenant = await pmsDbContext.Tenants
            .Where(x => x.Id == tenantAccount.TenantId)
            .FirstOrDefaultAsync();

        if (tenant == null)
            throw new InvalidOperationException("Tenant not found");

        // Update tenant information
        tenant.Name = editTenantProfile.Name;
        tenant.PhoneNumber = editTenantProfile.PhoneNumber;
        tenant.Email = editTenantProfile.Email;

        // Update tenant account email if it's different
        if (tenantAccount.Email != editTenantProfile.Email)
        {
            tenantAccount.Email = editTenantProfile.Email;
        }

        await pmsDbContext.SaveChangesAsync();
    }

    public async Task ChangeTenantPassword(Guid tenantAccountId, ChangeTenantPasswordRequest request)
    {
        var tenantAccount = await pmsDbContext.TenantAccounts.FirstOrDefaultAsync(x => x.Id == tenantAccountId);
        if (tenantAccount == null)
            throw new InvalidOperationException("Tenant account not found");

        // Validate current password
        if (!VerifyPassword(request.CurrentPassword, tenantAccount.HashedPassword))
            throw new UnauthorizedAccessException("Current password is incorrect");

        // Update password
        tenantAccount.HashedPassword = HashPassword(request.NewPassword);
        await pmsDbContext.SaveChangesAsync();
    }

    private static string HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashedBytes = System.Text.Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(hashedBytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    private static bool VerifyPassword(string password, string hashedPassword)
    {
        return HashPassword(password) == hashedPassword;
    }
}
