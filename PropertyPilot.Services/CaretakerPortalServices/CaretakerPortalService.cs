using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Services.CaretakerPortalServices.Models;
using PropertyPilot.Services.CaretakerPortalServices.Models.Responses;

namespace PropertyPilot.Services.CaretakerPortalServices;

public class CaretakerPortalService(PmsDbContext pmsDbContext)
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
}
