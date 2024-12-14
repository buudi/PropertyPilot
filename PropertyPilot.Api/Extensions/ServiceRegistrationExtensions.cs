using PropertyPilot.Services.JwtServices;
using PropertyPilot.Services.PropertiesServices;
using PropertyPilot.Services.TenantsServices;

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
        services.AddScoped<PropertiesService>();
        services.AddScoped<TenantsService>();
        //services.AddScoped<UserService>();
        services.AddScoped<JwtService>();
    }
}
