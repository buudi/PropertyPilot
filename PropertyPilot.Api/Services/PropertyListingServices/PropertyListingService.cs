using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;

namespace PropertyPilot.Api.Services.PropertyListingServices;

public class PropertyListingService(PmsDbContext pmsDbContext)
{
    public async Task<List<PropertyListing>> GetAllPropertyListings()
    {
        List<PropertyListing> listings = await pmsDbContext.PropertyListings.ToListAsync();
        return listings;
    }
}
