using PropertyPilot.Services.CaretakerPortalServices;
using PropertyPilot.Services.ContractsServices;
using PropertyPilot.Services.FinanceServices;
using PropertyPilot.Services.HostedServices;
using PropertyPilot.Services.JwtServices;
using PropertyPilot.Services.LookupServices;
using PropertyPilot.Services.PropertiesServices;
using PropertyPilot.Services.TenantServices;
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
        services.AddScoped<JwtService>();
        services.AddScoped<PropertiesService>();
        services.AddScoped<ContractsService>();
        services.AddScoped<UserService>();
        services.AddScoped<TenantService>();
        services.AddScoped<LookupService>();
        services.AddScoped<FinancesService>();
        services.AddScoped<CaretakerPortalService>();

        services.AddSingleton<IHostedService, InvoiceRenewHostedService>();
    }
}
