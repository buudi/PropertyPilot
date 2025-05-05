using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.CaretakerPortalServices.Models;
using PropertyPilot.Services.CaretakerPortalServices.Models.Finances;
using PropertyPilot.Services.CaretakerPortalServices.Models.Responses;
using PropertyPilot.Services.CaretakerPortalServices.Models.Settings;
using PropertyPilot.Services.Constants;
using PropertyPilot.Services.Extensions;
using PropertyPilot.Services.FinanceServices;
using PropertyPilot.Services.FinanceServices.Models;
using PropertyPilot.Services.Generics;

namespace PropertyPilot.Services.CaretakerPortalServices;

public class CaretakerPortalService(PmsDbContext pmsDbContext, FinancesService financesService)
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

            var assignedApartmentResponse = new AssignedApartment
            {
                Id = apartment?.Id ?? Guid.Empty,
                PropertyName = apartment?.PropertyName ?? string.Empty,
                PropertyAddress = apartment?.Emirate ?? string.Empty,
                VacanciesCount = 3, // Placeholder value
                SubUnitsCount = apartment?.UnitsCount ?? 0,
                OutstandingBalance = 300, // Placeholder value
                TenantsLeavingThisMonth = 2 // Placeholder value
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
}