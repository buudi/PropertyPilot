using PropertyPilot.Services.JwtServices;
using PropertyPilot.Services.PropertyListingServices;
using PropertyPilot.Services.UserServices;

namespace PropertyPilot.Api.Extensions;

/// <summary>
/// Register project services
/// </summary>
public static class ServiceRegistrationExtensions
{
    /// <summary>
    ///  add Property Pilot services
    /// </summary>
    /// <param name="services"></param>
    public static void AddPropertyPilotServices(this IServiceCollection services)
    {
        services.AddScoped<PropertiesListService>();
        services.AddScoped<UserService>();
        services.AddScoped<JwtService>();
    }
}
