using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using PropertyPilot.Dal.Contexts;
using System.Net;
using System.Text.Json;
using Xunit;
using FluentAssertions;

namespace PropertyPilot.Tests.Integration.Controllers;

public class AuthControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext registrations
                var pmsDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<PmsDbContext>));
                if (pmsDescriptor != null)
                {
                    services.Remove(pmsDescriptor);
                }

                var ppDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<PpDbContext>));
                if (ppDescriptor != null)
                {
                    services.Remove(ppDescriptor);
                }

                // Add in-memory databases for testing
                services.AddDbContext<PmsDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestPmsDb");
                });

                services.AddDbContext<PpDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestPpDb");
                });

                // Configure test JWT settings
                services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["Jwt:Key"] = "test-jwt-key-that-is-long-enough-for-hmac-sha256-testing-purposes",
                        ["Jwt:Issuer"] = "test-issuer",
                        ["Jwt:Audience"] = "test-audience"
                    })
                    .Build());
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        // This should return 401 Unauthorized since we're not authenticated, which is expected
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateAdminManagerAccount_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var signUpData = new
        {
            Name = "Test Admin",
            Email = "admin@test.com",
            Password = "TestPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/signups/admin-manager", signUpData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Admin Manager account created successfully");
    }

    [Fact]
    public async Task CreateAdminManagerAccount_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var signUpData = new
        {
            Name = "Test Admin",
            Email = "duplicate@test.com",
            Password = "TestPassword123!"
        };

        // Create first user
        await _client.PostAsJsonAsync("/api/auth/signups/admin-manager", signUpData);

        // Act - Try to create second user with same email
        var response = await _client.PostAsJsonAsync("/api/auth/signups/admin-manager", signUpData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("User with this email already exists");
    }
} 