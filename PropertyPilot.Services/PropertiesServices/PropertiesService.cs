using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.PropertiesServices.Models;

namespace PropertyPilot.Services.PropertiesServices;

public class PropertiesService(PpDbContext ppDbContext)
{
    public async Task<List<Property>> GetAllPropertyAsync()
    {
        List<Property> properties = await ppDbContext.Properties.AsNoTracking().ToListAsync();
        return properties;
    }

    public async Task<Property?> GetPropertyByIdAsync(Guid Id)
    {
        Property? property = await ppDbContext.Properties
            .Where(x => x.Id == Id)
            .FirstOrDefaultAsync();

        return property;
    }

    public Property CreateProperty(CreatePropertyRequest createPropertyRequest)
    {
        var newProperty = new Property
        {
            PropertyName = createPropertyRequest.PropertyName,
            Emirate = createPropertyRequest.Emirate,
            PropertyType = createPropertyRequest.PropertyType,
            UnitsCount = createPropertyRequest.UnitsCount
        };

        ppDbContext.Properties.Add(newProperty);
        ppDbContext.SaveChanges();

        return newProperty;
    }

    public async Task UpdatePropertyAsync(Guid id, UpdatePropertyRequest updatePropertyRequest)
    {
        Property? existingProperty = await ppDbContext
            .Properties
            .FindAsync(id);

        if (existingProperty == null)
        {
            return;
        }

        existingProperty.PropertyName = updatePropertyRequest.PropertyName;
        existingProperty.Emirate = updatePropertyRequest.Emirate;
        existingProperty.PropertyType = updatePropertyRequest.PropertyType;
        existingProperty.UnitsCount = updatePropertyRequest.UnitsCount;

        await ppDbContext.SaveChangesAsync();
    }

    public List<Property> CreateProperties(List<CreatePropertyRequest> createPropertyRequests)
    {
        // Validate the input to ensure it's not null or empty
        if (createPropertyRequests == null || !createPropertyRequests.Any())
        {
            throw new ArgumentException("The list of property requests cannot be null or empty.", nameof(createPropertyRequests));
        }

        // Create a list to hold the new properties
        var newProperties = createPropertyRequests.Select(request => new Property
        {
            PropertyName = request.PropertyName,
            Emirate = request.Emirate,
            PropertyType = request.PropertyType,
            UnitsCount = request.UnitsCount
        }).ToList();

        // Add all properties to the database context
        ppDbContext.Properties.AddRange(newProperties);

        // Save changes to persist the data in the database
        ppDbContext.SaveChanges();

        return newProperties;
    }

}
