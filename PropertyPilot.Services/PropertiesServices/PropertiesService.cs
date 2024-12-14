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
}
