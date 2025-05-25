using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.CaretakerPortalServices.Models;
using PropertyPilot.Services.CaretakerPortalServices.Models.Finances;
using PropertyPilot.Services.CaretakerPortalServices.Models.Properties;
using PropertyPilot.Services.CaretakerPortalServices.Models.Properties.TenantPage;
using PropertyPilot.Services.CaretakerPortalServices.Models.Responses;
using PropertyPilot.Services.CaretakerPortalServices.Models.Settings;
using PropertyPilot.Services.Constants;
using PropertyPilot.Services.Extensions;
using PropertyPilot.Services.FinanceServices;
using PropertyPilot.Services.FinanceServices.Models;
using PropertyPilot.Services.Generics;
using PropertyPilot.Services.InvoiceServices.Models;
using PropertyPilot.Services.PropertiesServices;
using PropertyPilot.Services.TenantServices;

namespace PropertyPilot.Services.CaretakerPortalServices;

public class CaretakerPortalService(PmsDbContext pmsDbContext, FinancesService financesService, PropertiesService propertiesService, TenantService tenantService)
{
    public async Task<CaretakerPortalHomeScreen> CaretakerPortalHomeScreen(Guid userId)
    {
        var caretaker = await pmsDbContext
            .PropertyPilotUsers
            .Where(x => x.Id == userId)
            .FirstOrDefaultAsync();

        var MonetaryAccount = await pmsDbContext
            .MonetaryAccounts
            .Where(x => x.UserId == userId)
            .FirstOrDefaultAsync();

        var assignedApartmentsIds = await pmsDbContext
            .AssignedCaretakerProperties
            .Where(x => x.UserId == userId)
            .Select(x => x.PropertyListingId)
            .ToListAsync();

        var assignedApartmentResponses = new List<AssignedApartment>();
        foreach (var apartmentId in assignedApartmentsIds)
        {
            var apartment = await pmsDbContext
                .PropertyListings
                .Where(x => x.Id == apartmentId)
                .FirstOrDefaultAsync();

            var apartmentTotalOutstandingBalance = await financesService.PropertyOutstandingSum(apartmentId);

            var vacanciesCount = await propertiesService.GetNumberOfVacanciesForProperty(apartmentId);
            var leavingThisMonthCount = await tenantService.ThisMonthEvacuatingTenantsCount(apartmentId);

            var assignedApartmentResponse = new AssignedApartment
            {
                Id = apartment?.Id ?? Guid.Empty,
                PropertyName = apartment?.PropertyName ?? string.Empty,
                PropertyAddress = apartment?.Emirate ?? string.Empty,
                VacanciesCount = vacanciesCount,
                SubUnitsCount = apartment?.UnitsCount ?? 0,
                OutstandingBalance = apartmentTotalOutstandingBalance,
                TenantsLeavingThisMonth = leavingThisMonthCount
            };

            assignedApartmentResponses.Add(assignedApartmentResponse);
        }

        var response = new CaretakerPortalHomeScreen
        {
            CaretakerFirstName = caretaker?.Name ?? string.Empty,
            AccountBalance = MonetaryAccount!.Balance,
            AssignedApartmentsList = assignedApartmentResponses
        };

        return response;
    }

    public async Task<CaretakerPortalFinancesScreen> CaretakerPortalFinancesScreen(Guid userId, int pageSize, int pageNumber = 1)
    {
        var caretaker = await pmsDbContext
            .PropertyPilotUsers
            .Where(x => x.Id == userId)
            .FirstOrDefaultAsync();

        var monetaryAccount = await pmsDbContext
            .MonetaryAccounts
            .Where(x => x.UserId == userId)
            .FirstOrDefaultAsync();

        var transactionsQuery = pmsDbContext
            .Transactions
            .Where(x => x.SourceAccountId == monetaryAccount!.Id || x.DestinationAccountId == monetaryAccount.Id);

        var totalItems = await transactionsQuery.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var transactionsPaginated = await transactionsQuery
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var collectedThisMonth = transactionsPaginated
            .Where(x => x.CreatedAt.Month == DateTime.UtcNow.Month && x.CreatedAt.Year == DateTime.UtcNow.Year)
            .Where(x => x.DestinationAccountId == monetaryAccount!.Id)
            .Where(x => x.TransactionType == Transaction.TransactionTypes.RentPayment)
            .Sum(x => x.Amount);

        var transactionHistoryRecords = new List<TransactionHistoryViewRecord>();
        foreach (var transaction in transactionsPaginated)
        {
            var transactionRecord = await transaction.AsTransactionRecord(pmsDbContext);

            string? referenceText = null;
            string? personName = null;
            if (transaction.TransactionType == Transaction.TransactionTypes.RentPayment)
            {
                var propertyListing = await financesService.GetPropertyListingFromRentPayment(transaction.ReferenceId);

                referenceText = propertyListing?.PropertyName;

                var tenant = await financesService.GetTenantFromRentPayment(transaction.ReferenceId);
                personName = tenant?.Name;
            }

            if (transaction.TransactionType == Transaction.TransactionTypes.Expense)
            {
                var propertyListing = await financesService.GetPropertyLisitngFromExpense(transaction.ReferenceId);

                referenceText = propertyListing?.PropertyName;
            }

            var transactionAmount = transactionRecord.Amount;
            if (transactionRecord.DestinationAccountId == Keys.MainMonetaryAccountGuid)
            {
                transactionAmount = -transactionAmount; // if transaction is to main account this amount would be negative
            }

            var transactionHistoryRecord = new TransactionHistoryViewRecord
            {
                TransactionId = transactionRecord.Id,
                TransactionType = transactionRecord.TransactionType,
                ReferenceId = transactionRecord.ReferenceId.ToString(),
                ReferenceText = referenceText,
                PersonName = personName,
                ToAccountName = transactionRecord.DestinationAccountName,
                Amount = transactionAmount, // if transaction is to main account this amount would be negative
                CreatedAt = transactionRecord.CreatedAt,
                Notes = "Place Holder Notes Value"
            };

            transactionHistoryRecords.Add(transactionHistoryRecord);
        }

        var paginatedHistoryRecords = new PaginatedResult<TransactionHistoryViewRecord>
        {
            Items = transactionHistoryRecords,
            TotalItems = totalItems,
            PageSize = pageSize,
            PageNumber = pageNumber,
            TotalPages = totalPages
        };

        var response = new CaretakerPortalFinancesScreen
        {
            CurrentBalance = monetaryAccount!.Balance,
            CollectedThisMonth = collectedThisMonth,
            TransactionHistoryRecords = paginatedHistoryRecords
        };

        return response;
    }

    public async Task<AttemptResult<Transaction>> RecordDeposit(Guid userId, double amount)
    {
        var monetaryAccount = await pmsDbContext
            .MonetaryAccounts
            .Where(x => x.UserId == userId)
            .FirstOrDefaultAsync();

        var transferRequest = new CreateTransferRequest
        {
            SourceAccountId = monetaryAccount!.Id,
            DestinationAccountId = Keys.MainMonetaryAccountGuid,
            Amount = amount
        };

        var attemptResult = await financesService.RecordTransferAsync(transferRequest);
        return attemptResult;
    }

    public async Task<CaretakerSettingsProfile> CaretakerPortalProfile(Guid userId)
    {
        var caretaker = await pmsDbContext
            .PropertyPilotUsers
            .Where(x => x.Id == userId)
            .FirstOrDefaultAsync();
        var response = new CaretakerSettingsProfile
        {
            Name = caretaker?.Name ?? string.Empty,
            Email = caretaker?.Email ?? string.Empty,
            PhoneNumber = "+971 50 123 4567", // Placeholder value
            MemberSince = caretaker?.CreatedOn.ToString("dd/MM/yyyy") ?? string.Empty
        };
        return response;
    }

    public async Task EditCaretakerProfile(Guid userId, EditCaretakerProfile editCaretakerProfile)
    {
        var caretaker = await pmsDbContext
            .PropertyPilotUsers
            .Where(x => x.Id == userId)
            .FirstOrDefaultAsync();
        if (caretaker != null)
        {
            caretaker.Name = editCaretakerProfile.Name;
            caretaker.Email = editCaretakerProfile.Email;
            // todo: update phone number
            await pmsDbContext.SaveChangesAsync();
        }
    }

    public async Task<List<TenantTabListing>> GetPropertiesTenantTabListing(Guid propertyId)
    {
        var tenancies = await pmsDbContext
            .Tenancies
            .Where(x => x.IsTenancyActive == true)
            .Where(x => x.PropertyListingId == propertyId)
            .ToListAsync();

        var tenantTabListings = new List<TenantTabListing>();

        foreach (var tenancy in tenancies)
        {
            var tenant = await pmsDbContext
                .Tenants
                .Where(x => x.Id == tenancy.TenantId)
                .FirstOrDefaultAsync();

            if (tenant == null)
            {
                continue; // Skip if tenancy is not found
            }

            var subUnitIdentifierName = await pmsDbContext
                .SubUnits
                .Where(x => x.Id == tenancy.SubUnitId)
                .Select(x => x.IdentifierName)
                .FirstOrDefaultAsync();

            // Calculate the next lease renewal date tenancy.RenewalPeriodInDays + todays date
            DateTime? nextLeaseRenewDate = null;
            if (tenancy.IsRenewable == true && tenancy.RenewalPeriodInDays != null)
            {
                nextLeaseRenewDate = DateTime.UtcNow.AddDays((double)tenancy.RenewalPeriodInDays);
            }

            var isLeavingThisMonth = false;

            if (tenancy.TenancyEnd != null)
            {
                var today = DateTime.UtcNow.Date;
                var endDate = ((DateTime)tenancy.TenancyEnd).Date;
                isLeavingThisMonth = endDate.Month == today.Month && endDate.Year == today.Year;
            }

            var tenantOutstanding = await financesService.IsTenantOutstanding(tenant.Id);

            var tenantTabListing = new TenantTabListing()
            {
                TenancyId = tenancy.TenantId,
                Name = tenant.Name,
                UnitNumber = subUnitIdentifierName ?? "no sub unit",
                LeaseEndDateTime = tenancy.TenancyEnd,
                IsLeaseAutoRenewable = tenancy.IsRenewable,
                NextLeaseRenewDate = nextLeaseRenewDate,
                IsLeavingThisMonth = isLeavingThisMonth,
                HasOutstandingBalance = tenantOutstanding.IsOutstanding,
                AmountDue = tenantOutstanding.OutstandingAmount
            };

            tenantTabListings.Add(tenantTabListing);
        }

        return tenantTabListings;
    }

    public async Task<PaginatedResult<InvoicesTabListing>> GetPropertiesInvoicesTabAsync(Guid propertyId, int pageSize, int pageNumber)
    {
        var tenanciesIds = await pmsDbContext
            .Tenancies
            .Where(x => x.PropertyListingId == propertyId)
            .Select(x => x.Id)
            .ToListAsync();

        var totalItems = await pmsDbContext
            .Invoices
            .Where(x => tenanciesIds.Contains(x.TenancyId))
            .CountAsync();

        var invoices = await pmsDbContext
            .Invoices
            .Where(x => tenanciesIds.Contains(x.TenancyId))
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
            var autoGenerated = true; // tempory place holder

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

        return new PaginatedResult<InvoicesTabListing>()
        {
            Items = invoicesTabListings,
            PageNumber = pageNumber,
            TotalItems = totalItems,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<PaginatedResult<PaymentsTabListing>> GetPaymentsTabAsync(Guid propertyId, int pageSize, int pageNumber)
    {
        var tenanciesIds = await pmsDbContext
            .Tenancies
            .Where(x => x.PropertyListingId == propertyId)
            .Select(x => x.Id)
            .ToListAsync();

        var invoicesIds = await pmsDbContext
            .Invoices
            .Where(x => tenanciesIds.Contains(x.TenancyId))
            .Select(x => x.Id)
            .ToListAsync();

        var totalItems = await pmsDbContext
            .RentPayments
            .Where(x => x.InvoiceId != null && invoicesIds.Contains(x.InvoiceId.Value))
            .CountAsync();

        var rentPayments = await pmsDbContext
            .RentPayments
            .Where(x => x.InvoiceId != null && invoicesIds.Contains(x.InvoiceId.Value))
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var paymentsTabListings = new List<PaymentsTabListing>();
        foreach (var rentPayment in rentPayments)
        {
            var tenant = await pmsDbContext
                .Tenants
                .Where(x => x.Id == rentPayment.TenantId)
                .FirstOrDefaultAsync();

            if (tenant == null)
            {
                continue;
            }

            var tenancyId = await pmsDbContext
                .Invoices
                .Where(x => x.Id == rentPayment.InvoiceId)
                .Select(x => x.TenancyId)
                .FirstOrDefaultAsync();

            var subUnitId = await pmsDbContext
                .Tenancies
                .Where(x => x.Id == tenancyId)
                .Select(x => x.SubUnitId)
                .FirstOrDefaultAsync();


            var subUnit = await pmsDbContext
                .SubUnits
                .Where(x => x.Id == subUnitId)
                .FirstOrDefaultAsync();

            var paymentsTabListing = new PaymentsTabListing()
            {
                PaymentId = rentPayment.Id,
                TenantName = tenant.Name,
                SubUnitName = subUnit!.IdentifierName,
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

    public async Task<PaginatedResult<ExpensesTabListing>> GetExpenseTabListing(Guid propertyId, int pageSize, int pageNumber)
    {
        var query = pmsDbContext
            .Expenses
            .Where(x => x.PropertyListingId == propertyId)
            .OrderByDescending(x => x.CreatedAt);

        var totalItems = await query.CountAsync();

        var expenses = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var expensesTabListings = new List<ExpensesTabListing>();
        foreach (var expense in expenses)
        {
            var expenseTabListing = new ExpensesTabListing
            {
                ExpenseId = expense.Id,
                ExpenseDescription = expense.Description ?? "No Description",
                ExpenseDate = expense.CreatedAt,
                Amount = expense.Amount,
                Category = expense.Category
            };

            expensesTabListings.Add(expenseTabListing);
        }

        return new PaginatedResult<ExpensesTabListing>
        {
            Items = expensesTabListings,
            TotalItems = totalItems,
            PageSize = pageSize,
            PageNumber = pageNumber,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<List<SubunitsTabListing>> GetSubunitsTab(Guid propertyId)
    {
        var subunits = await pmsDbContext
            .SubUnits
            .Where(x => x.PropertyListingId == propertyId)
            .OrderBy(x => x.IdentifierName)
            .ToListAsync();

        var subunitIds = subunits.Select(x => x.Id).ToList();

        var tenancies = await pmsDbContext.Tenancies
            .Where(x => x.IsTenancyActive && subunitIds.Contains((Guid)x.SubUnitId!))
            .ToListAsync();

        var tenantIds = tenancies.Select(x => x.TenantId).Distinct().ToList();

        var tenants = await pmsDbContext.Tenants
            .Where(x => tenantIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id);

        var subunitTabListings = subunits.Select(subunit =>
        {
            var tenancy = tenancies.FirstOrDefault(t => t.SubUnitId == subunit.Id);
            tenants.TryGetValue(tenancy?.TenantId ?? Guid.Empty, out var tenant);

            return new SubunitsTabListing
            {
                SubunitId = subunit.Id,
                SubunitName = subunit.IdentifierName,
                isOccupied = tenancy != null,
                TenantName = tenant?.Name
            };
        }).ToList();

        return subunitTabListings;
    }

    // record payment

    public async Task<AttemptResult<RentPaymentTransactionRecord>> RecordRentPayment(Guid userId, RentPaymentRequest request)
    {
        return await financesService.RecordRentPayment(userId, request);
    }

    // make expense

    public async Task<AttemptResult<ExpenseTransactionRecord>> RecordExpenseAsync(Guid propertyId, Guid userId, RecordExpenseCaretaker request)
    {
        var createExpenseRequest = new CreateExpenseRequest()
        {
            PropertyListingId = propertyId,
            PaidByUserId = userId,
            Category = request.Category,
            Description = request.Description,
            Amount = request.Amount
        };

        return await financesService.RecordExpenseAsync(createExpenseRequest);
    }

    // add an invoice
    public async Task<InvoiceRecord> CreateInvoiceRecord(CreateInvoiceRequest createInvoiceRequest)
    {
        var invoiceRecord = await financesService.CreateInvoiceRecord(createInvoiceRequest);
        return invoiceRecord;
    }

    // get tenant financial summary
    public async Task<TenantFinancialSummary> GetTenantFinancialSummary(Guid tenancyId)
    {
        var tenantOutstanding = await financesService.TenantOutstandingSum(tenancyId);
        var totalPaid = await financesService.TenantTotalPaidRent(tenancyId);
        var (lastPaymentDate, lastPaymentAmount) = await financesService.TenantLastPaidRentDateAndAmount(tenancyId);
        var stayDuration = await tenantService.GetStayDuration(tenancyId);

        return new TenantFinancialSummary
        {
            TotalOwed = tenantOutstanding,
            TotalPaid = totalPaid,
            LastPaymentDate = lastPaymentDate,
            LastPaymentAmount = lastPaymentAmount,
            StayDuration = stayDuration
        };
    }

}
