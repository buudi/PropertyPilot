using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.Extensions;
using PropertyPilot.Services.Generics;
using PropertyPilot.Services.PropertiesServices.Models;
using PropertyPilot.Services.PropertyListingServices.Models;

namespace PropertyPilot.Services.PropertiesServices;

public class PropertiesService(PpDbContext ppDbContext, PmsDbContext pmsDbContext)
{
    public async Task<PaginatedResult<PropertyListingRecord>> GetAllPropertyAsync(int pageNumber, int pageSize)
    {
        var totalProperties = await pmsDbContext.PropertyListings.CountAsync();

        List<PropertyListing> properties = await pmsDbContext.PropertyListings
            .AsNoTracking()
            .OrderBy(u => u.PropertyName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var propertyListings = new List<PropertyListingRecord>();

        foreach (var property in properties)
        {
            var propertyListing = await property.AsPropertyListingRecord(pmsDbContext);

            propertyListings.Add(propertyListing);
        }

        return new PaginatedResult<PropertyListingRecord>
        {
            Items = propertyListings,
            TotalItems = totalProperties,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalProperties / (double)pageSize)
        };
    }

    public PropertiesDashboardRecord GetPropertiesDashboard()
    {
        var propertyDashboard = new PropertiesDashboardRecord
        {
            TotalProperties = 12,
            VacantUnits = 9,
            OccupancyRate = 78.8,
            AverageMonthlyRevenue = 44000
        };

        return propertyDashboard;
    }

    public async Task<PropertyListingRecord?> GetPropertyByIdAsync(Guid Id)
    {
        var property = await pmsDbContext.PropertyListings
            .Where(x => x.Id == Id)
            .FirstOrDefaultAsync();

        if (property == null)
        {
            return null;
        }

        return await property.AsPropertyListingRecord(pmsDbContext);
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

    public async Task PopulateSubUnitsAsync()
    {
        // get all properties where UnitsCount > 0
        var properties = await pmsDbContext.PropertyListings.Where(x => x.UnitsCount > 0).ToListAsync();

        // create SubUnits for that count with IdentifierName "A1, A2" and so on
        foreach (var property in properties)
        {
            var propertyId = property.Id;
            var unitsCount = property.UnitsCount;

            var existingSubs = await pmsDbContext.SubUnits.Where(x => x.PropertyListingId == propertyId).ToListAsync();
            var existingSubsCount = existingSubs.Count;

            if (existingSubsCount >= unitsCount)
            {
                continue;
            }

            for (var i = 0; i < unitsCount - existingSubsCount; i++)
            {
                var subUnit = new SubUnit
                {
                    PropertyListingId = propertyId,
                    IdentifierName = $"S{i + 1}"
                };

                pmsDbContext.SubUnits.Add(subUnit);
            }
        }

        await pmsDbContext.SaveChangesAsync();
    }

    public async Task<TimelineResponse> GetPropertyTenantsTimelineAsync(Guid propertyId)
    {
        var subUnits = await pmsDbContext.SubUnits
            .Where(x => x.PropertyListingId == propertyId)
            .ToListAsync();

        var timelineSubunits = subUnits
            .Select(subUnit => new SubUnitTimelineResponse
            {
                SubUnitId = subUnit.Id,
                SubUnitIdentifierName = subUnit.IdentifierName
            })
            .ToList();

        var tenancies = await pmsDbContext.Tenancies
            .Where(x => x.PropertyListingId == propertyId)
            .ToListAsync();

        var timelineTenants = new List<TenantsTimelineResponse>();
        foreach (var tenancy in tenancies)
        {
            var tenant = await pmsDbContext.Tenants
                .Where(x => x.Id == tenancy.TenantId)
                .FirstOrDefaultAsync();

            timelineTenants.Add(new TenantsTimelineResponse
            {
                TenantId = tenant.Id,
                TenantName = tenant.Name,
                TenancyStart = tenancy.TenancyStart,
                TenancyEnd = tenancy.TenancyEnd,
                SubUnitId = tenancy.SubUnitId ?? Guid.Empty,
            });
        }

        return new TimelineResponse
        {
            SubUnitTimelineResponse = timelineSubunits,
            TenantsTimelineResponse = timelineTenants
        };
    }
}
