using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
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
}
