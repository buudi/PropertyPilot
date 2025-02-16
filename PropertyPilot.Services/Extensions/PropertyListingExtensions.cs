using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.PropertiesServices.Models;

namespace PropertyPilot.Services.Extensions;

public static class PropertyListingExtensions
{
    public static async Task<PropertyListingRecord> AsPropertyListingRecord(this PropertyListing propertyListing,
        PmsDbContext pmsDbContext)
    {
        var caretaker = await pmsDbContext.PropertyPilotUsers.Where(x => x.Id == Guid.Parse("4e626d3c-edd4-4712-859c-2e72722d9db6")).FirstOrDefaultAsync();
        var propertyListingRecord = new PropertyListingRecord
        {
            Id = propertyListing.Id,
            PropertyName = propertyListing.PropertyName,
            Emirate = propertyListing.Emirate,
            PropertyType = propertyListing.PropertyType,
            // todo: map correct occupancy based on propertyListing type
            Occupancy = propertyListing.PropertyType == Property.PropertyTypes.Singles
                ? "3/5"
                : "100%",
            UnitsCount = propertyListing.UnitsCount,
            CaretakerId = caretaker?.Id,
            CaretakerName = caretaker?.Name,
            CaretakerEmail = caretaker?.Email
        };

        return propertyListingRecord;
    }
}