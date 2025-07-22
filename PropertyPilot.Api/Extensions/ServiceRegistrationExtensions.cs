using PropertyPilot.Services.CaretakerPortalServices;
using PropertyPilot.Services.ContractsServices;
using PropertyPilot.Services.FinanceServices;
using PropertyPilot.Services.HostedServices;
using PropertyPilot.Services.JwtServices;
using PropertyPilot.Services.LookupServices;
using PropertyPilot.Services.MigrationServices;
using PropertyPilot.Services.PropertiesServices;
using PropertyPilot.Services.TenantPortalServices;
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
        services.AddScoped<MigrationService>();
        services.AddScoped<TenantPortalService>();

        services.AddSingleton<IHostedService, InvoiceRenewHostedService>();
    }

    /// <summary>
    /// Add Swagger configuration
    /// </summary>
    public static void AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "PropertyPilot API", Version = "v1" });

            // Set the comments path for the Swagger JSON and UI.
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = System.IO.Path.Combine(System.AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);

            // Configure Swagger to use JWT Authentication
            var securityScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Name = "JWT Authentication",
                Description = "Enter JWT Bearer token **_only_**",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Id = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme
                }
            };
            c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
            c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                { securityScheme, new[] { "Bearer" } }
            });
        });
    }

    /// <summary>
    /// Add CORS configuration
    /// </summary>
    public static void AddDefaultCors(this IServiceCollection services, string policyName)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(name: policyName, policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
    }
}
