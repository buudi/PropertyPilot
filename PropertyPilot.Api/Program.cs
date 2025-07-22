using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PropertyPilot.Api.Constants;
using PropertyPilot.Api.Extensions;
using PropertyPilot.Dal.Contexts;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Use extension method for Swagger
builder.Services.AddSwaggerDocumentation();

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
// Use extension method for CORS
builder.Services.AddDefaultCors(MyAllowSpecificOrigins);

builder.Services.AddDbContext<PmsDbContext>();
builder.Services.AddDbContext<PpDbContext>();
builder.Services.AddPropertyPilotServices();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        NameClaimType = ClaimTypes.Email,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthPolicies.AdminManagerOnly, policy => policy.RequireRole
            (PropertyPilot.Dal.Models.PropertyPilotUser.UserRoles.AdminManager));

    options.AddPolicy(AuthPolicies.CaretakerOnly, policy => policy.RequireRole(PropertyPilot.Dal.Models.PropertyPilotUser.UserRoles.Caretaker));

    options.AddPolicy(AuthPolicies.ManagerAndAbove, policy => policy.RequireRole
            (PropertyPilot.Dal.Models.PropertyPilotUser.UserRoles.AdminManager,
            PropertyPilot.Dal.Models.PropertyPilotUser.UserRoles.Manager));

    options.AddPolicy(AuthPolicies.TenantOnly, policy => policy.RequireRole(PropertyPilot.Dal.Models.PropertyPilotUser.UserRoles.Tenant));

    options.AddPolicy(AuthPolicies.AllRoles, policy => policy.RequireRole
            (PropertyPilot.Dal.Models.PropertyPilotUser.UserRoles.AdminManager,
            PropertyPilot.Dal.Models.PropertyPilotUser.UserRoles.Manager,
            PropertyPilot.Dal.Models.PropertyPilotUser.UserRoles.Caretaker,
            PropertyPilot.Dal.Models.PropertyPilotUser.UserRoles.Tenant));
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PropertyPilot API v1"));
}

app.UseHttpsRedirection();

app.UseDeveloperExceptionPage();
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(error, "Unhandled exception");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Unhandled exception");
    });
});

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var stopwatch = Stopwatch.StartNew();
app.Run();
stopwatch.Stop();
Console.WriteLine($"[Startup Timing] Application startup completed in {stopwatch.ElapsedMilliseconds} ms");

// Make Program class public for integration tests
public partial class Program { }