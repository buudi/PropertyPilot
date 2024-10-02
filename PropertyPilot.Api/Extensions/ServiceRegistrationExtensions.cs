using PropertyPilot.Dal.Contexts;
using PropertyPilot.Services.PropertyListingServices;
using PropertyPilot.Services.UserServices;

namespace PropertyPilot.Api.Extensions;

public static class ServiceRegistrationExtensions
{
    public static void AddPropertyPilotServices(this IServiceCollection services)
    {
        services.AddDbContext<PmsDbContext>();

        services.AddScoped<PropertyListingService>();
        services.AddScoped<UserService>();
    }
}
