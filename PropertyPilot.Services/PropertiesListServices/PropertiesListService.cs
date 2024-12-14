using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.PropertyListingServices.Models;

namespace PropertyPilot.Services.PropertyListingServices;

public class PropertiesListService(PpDbContext ppDbContext)
{
    public async Task<List<Property>> GetAllPropertyListingsAsync()
    {
        List<Property> listings = await ppDbContext.Properties.AsNoTracking().ToListAsync();
        return listings;
    }

    public async Task<Property?> GetPropertyListingByIdAsync(Guid Id)
    {
        Property? listing = await ppDbContext.Properties
            .Where(x => x.Id == Id)
            .FirstOrDefaultAsync();

        return listing;
    }

    public Property CreatePropertyListing(CreatePropertyListingRequest createListingRequest)
    {
        var newListing = new Property
        {
            PropertyName = createListingRequest.PropertyName,
            Emirate = createListingRequest.Emirate,
            PropertyType = createListingRequest.PropertyType,
            UnitsCount = createListingRequest.UnitsCount
        };

        ppDbContext.Properties.Add(newListing);
        ppDbContext.SaveChanges();

        return newListing;
    }

    public async Task UpdatePropertyListingAsync(Guid id, UpdatePropertyListingRequest updatePropertyListingRequest)
    {
        Property? existingListing = await ppDbContext
            .Properties
            .FindAsync(id);

        if (existingListing == null)
        {
            return;
        }

        existingListing.PropertyName = updatePropertyListingRequest.PropertyName;
        existingListing.Emirate = updatePropertyListingRequest.Emirate;
        existingListing.PropertyType = updatePropertyListingRequest.PropertyType;
        existingListing.UnitsCount = updatePropertyListingRequest.UnitsCount;

        await ppDbContext.SaveChangesAsync();
    }
}
