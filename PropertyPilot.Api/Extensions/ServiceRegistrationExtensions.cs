using PropertyPilot.Api.Services.PropertyListingServices;
using PropertyPilot.Dal.Contexts;

namespace PropertyPilot.Api.Extensions;

public static class ServiceRegistrationExtensions
{
    public static void AddPropertyPilotServices(this IServiceCollection services)
    {
        services.AddDbContext<PmsDbContext>();

        services.AddScoped<PropertyListingService>();
    }
}
