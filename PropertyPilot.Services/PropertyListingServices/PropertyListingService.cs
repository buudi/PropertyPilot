using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.PropertyListingServices.Models;

namespace PropertyPilot.Services.PropertyListingServices;

public class PropertyListingService(PpDbContext ppDbContext)
{
    public async Task<List<PropertiesList>> GetAllPropertyListingsAsync()
    {
        List<PropertiesList> listings = await ppDbContext.PropertiesList.AsNoTracking().ToListAsync();
        return listings;
    }

    public async Task<PropertiesList?> GetPropertyListingByIdAsync(Guid Id)
    {
        PropertiesList? listing = await ppDbContext.PropertiesList
            .Where(x => x.Id == Id)
            .FirstOrDefaultAsync();

        return listing;
    }

    public PropertiesList CreatePropertyListing(CreatePropertyListingRequest createListingRequest)
    {
        var newListing = new PropertiesList
        {
            PropertyName = createListingRequest.PropertyName,
            Emirate = createListingRequest.Emirate,
            PropertyType = createListingRequest.PropertyType,
            UnitsCount = createListingRequest.UnitsCount
        };

        ppDbContext.PropertiesList.Add(newListing);
        ppDbContext.SaveChanges();

        return newListing;
    }

    public async Task UpdatePropertyListingAsync(Guid id, UpdatePropertyListingRequest updatePropertyListingRequest)
    {
        PropertiesList? existingListing = await ppDbContext
            .PropertiesList
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
