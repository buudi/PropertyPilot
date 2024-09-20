using Microsoft.EntityFrameworkCore;
using PropertyPilot.Api.Services.PropertyListingServices.Models;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;

namespace PropertyPilot.Api.Services.PropertyListingServices;

public class PropertyListingService(PmsDbContext pmsDbContext)
{
    public async Task<List<PropertyListing>> GetAllPropertyListingsAsync()
    {
        List<PropertyListing> listings = await pmsDbContext.PropertyListings.AsNoTracking().ToListAsync();
        return listings;
    }

    public async Task<PropertyListing?> GetPropertyListingByIdAsync(Guid Id)
    {
        PropertyListing? listing = await pmsDbContext.PropertyListings
            .Where(x => x.Id == Id)
            .FirstOrDefaultAsync();

        return listing;
    }

    public PropertyListing CreatePropertyListing(CreatePropertyListingRequest createListingRequest)
    {
        var newListing = new PropertyListing
        {
            PropertyName = createListingRequest.PropertyName,
            Emirate = createListingRequest.Emirate,
            PropertyType = createListingRequest.PropertyType,
            UnitsCount = createListingRequest.UnitsCount
        };

        pmsDbContext.PropertyListings.Add(newListing);
        pmsDbContext.SaveChanges();

        return newListing;
    }

    public async Task UpdatePropertyListingAsync(Guid id, UpdatePropertyListingRequest updatePropertyListingRequest)
    {
        PropertyListing? existingListing = await pmsDbContext
            .PropertyListings
            .FindAsync(id);

        if (existingListing == null)
        {
            return;
        }

        existingListing.PropertyName = updatePropertyListingRequest.PropertyName;
        existingListing.Emirate = updatePropertyListingRequest.Emirate;
        existingListing.PropertyType = updatePropertyListingRequest.PropertyType;
        existingListing.UnitsCount = updatePropertyListingRequest.UnitsCount;

        await pmsDbContext.SaveChangesAsync();
    }
}
