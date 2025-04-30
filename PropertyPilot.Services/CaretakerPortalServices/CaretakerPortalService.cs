using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.CaretakerPortalServices.Models;
using PropertyPilot.Services.CaretakerPortalServices.Models.Finances;
using PropertyPilot.Services.CaretakerPortalServices.Models.Responses;
using PropertyPilot.Services.Constants;
using PropertyPilot.Services.Extensions;
using PropertyPilot.Services.FinanceServices;

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

    public async Task<CaretakerPortalFinancesScreen> CaretakerPortalFinancesScreen(Guid userId)
    {
        var caretaker = await pmsDbContext
            .PropertyPilotUsers
            .Where(x => x.Id == userId)
            .FirstOrDefaultAsync();

        var monetaryAccount = await pmsDbContext
            .MonetaryAccounts
            .Where(x => x.UserId == userId)
            .FirstOrDefaultAsync();

        var transactions = await pmsDbContext
            .Transactions
            .Where(x => x.SourceAccountId == monetaryAccount!.Id || x.DestinationAccountId == monetaryAccount.Id)
            .ToListAsync();

        var collectedThisMonth = transactions
            .Where(x => x.CreatedAt.Month == DateTime.UtcNow.Month && x.CreatedAt.Year == DateTime.UtcNow.Year)
            .Where(x => x.DestinationAccountId == monetaryAccount!.Id)
            .Where(x => x.TransactionType == Transaction.TransactionTypes.RentPayment)
            .Sum(x => x.Amount);

        var transactionHistoryRecords = new List<TransactionHistoryViewRecord>();
        foreach (var transaction in transactions)
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

        var response = new CaretakerPortalFinancesScreen
        {
            CurrentBalance = monetaryAccount!.Balance,
            CollectedThisMonth = collectedThisMonth,
            TransactionHistoryRecords = transactionHistoryRecords
        };

        return response;
    }
}
