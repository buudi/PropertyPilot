using Microsoft.EntityFrameworkCore;
using PropertyPilot.Api.Services.PropertyListingServices.Models;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;

namespace PropertyPilot.Api.Services.PropertyListingServices;

public class PropertyListingService(PmsDbContext pmsDbContext)
{
    public async Task<List<PropertyListing>> GetAllPropertyListings()
    {
        List<PropertyListing> listings = await pmsDbContext.PropertyListings.AsNoTracking().ToListAsync();
        return listings;
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
}
