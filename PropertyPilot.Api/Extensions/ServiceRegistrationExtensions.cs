using PropertyPilot.Dal.Contexts;
using PropertyPilot.Services.PropertyListingServices;

namespace PropertyPilot.Api.Extensions;

public static class ServiceRegistrationExtensions
{
    public static void AddPropertyPilotServices(this IServiceCollection services)
    {
        services.AddDbContext<PmsDbContext>();

        services.AddScoped<PropertyListingService>();
    }
}
