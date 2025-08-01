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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PropertyPilot API", Version = "v1" });

    // Set the comments path for the Swagger JSON and UI.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // Configure Swagger to use JWT Authentication
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter JWT Bearer token **_only_**",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        //{securityScheme, new string[] { }}
        { securityScheme, new[] { "Bearer" } }
    });
});

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// temporarily allow all origins
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "_myAllowSpecificOrigins", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy(name: "_myAllowSpecificOrigins", policy =>
//    {
//        policy.WithOrigins("http://localhost:5173")
//            .AllowAnyHeader()
//            .AllowAnyMethod();

//        policy.WithOrigins("http://localhost:5174")
//            .AllowAnyHeader()
//            .AllowAnyMethod();
//    });
//});

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

app.Run();
