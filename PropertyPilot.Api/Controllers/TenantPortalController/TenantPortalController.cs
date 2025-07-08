using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyPilot.Api.Extensions;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.TenantPortalServices;
using PropertyPilot.Services.TenantPortalServices.Models.Settings;

namespace PropertyPilot.Api.Controllers.TenantPortalController;

[Route("api/tenants-portal")]
[ApiController]
public class TenantPortalController(TenantPortalService tenantPortalService, PmsDbContext pmsDbContext) : ControllerBase
{
    [HttpGet("settings/basic-info")]
    public async Task<IActionResult> GetBasicInfo()
    {
        if (!HttpContext.User.Identity?.IsAuthenticated ?? true)
            return Unauthorized(new { error = "Unauthorized" });

        var userId = HttpContext.GetUserId();
        var response = await tenantPortalService.GetBasicTenantInfo(userId);

        if (response == null)
            return NotFound("Tenant Not Found");

        return Ok(response);
    }

    [HttpGet("tenancy/current")]
    public async Task<IActionResult> GetCurrentActiveTenancy()
    {
        if (!HttpContext.User.Identity?.IsAuthenticated ?? true)
            return Unauthorized(new { error = "Unauthorized" });

        var tenantAccountId = HttpContext.GetUserId();
        var tenancyInfo = await tenantPortalService.GetCurrentActiveTenancyInfo(tenantAccountId);

        if (tenancyInfo == null)
            return NotFound("No active tenancy found for this tenant.");

        return Ok(tenancyInfo);
    }

    [HttpGet("finances/outstanding-amount")]
    public async Task<IActionResult> GetOutstandingAmount()
    {
        if (!HttpContext.User.Identity?.IsAuthenticated ?? true)
            return Unauthorized(new { error = "Unauthorized" });

        var tenantAccountId = HttpContext.GetUserId();
        var response = await tenantPortalService.GetOutstandingAmount(tenantAccountId);

        return Ok(new
        {
            OutstandingAmount = response
        });
    }

    [HttpGet("payments")]
    public async Task<IActionResult> GetAllPaymentsForTenant([FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 1)
    {
        if (!HttpContext.User.Identity?.IsAuthenticated ?? true)
            return Unauthorized(new { error = "Unauthorized" });

        var tenantAccountId = HttpContext.GetUserId();
        var result = await tenantPortalService.GetAllPaymentsForTenantAsync(tenantAccountId, pageSize, pageNumber);

        return Ok(result);
    }

    [HttpGet("invoices")]
    public async Task<IActionResult> GetAllInvoicesForTenant([FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 1)
    {
        if (!HttpContext.User.Identity?.IsAuthenticated ?? true)
            return Unauthorized(new { error = "Unauthorized" });

        var tenantAccountId = HttpContext.GetUserId();
        var result = await tenantPortalService.GetAllInvoicesForTenantAsync(tenantAccountId, pageSize, pageNumber);

        return Ok(result);
    }

    [HttpGet("caretaker/details")]
    public async Task<IActionResult> GetCaretakerDetails()
    {
        if (!HttpContext.User.Identity?.IsAuthenticated ?? true)
            return Unauthorized(new { error = "Unauthorized" });

        var tenantAccountId = HttpContext.GetUserId();
        var caretaker = await tenantPortalService.GetCaretakerDetailsForTenant(tenantAccountId);

        if (caretaker == null)
            return NotFound("Caretaker not found for this tenant.");

        return Ok(caretaker);
    }

    [HttpGet("activity/recent")]
    public async Task<IActionResult> GetRecentActivity([FromQuery] int limit = 10)
    {
        if (!HttpContext.User.Identity?.IsAuthenticated ?? true)
            return Unauthorized(new { error = "Unauthorized" });

        var tenantAccountId = HttpContext.GetUserId();
        var activity = await tenantPortalService.GetRecentActivityForTenant(tenantAccountId, limit);
        return Ok(activity);
    }

    [HttpGet("activity/paginated")]
    public async Task<IActionResult> GetPaginatedActivity([FromQuery] int pageSize = 30, [FromQuery] int pageNumber = 1)
    {
        if (!HttpContext.User.Identity?.IsAuthenticated ?? true)
            return Unauthorized(new { error = "Unauthorized" });

        var tenantAccountId = HttpContext.GetUserId();
        var activity = await tenantPortalService.GetPaginatedActivityForTenant(tenantAccountId, pageSize, pageNumber);
        return Ok(activity);
    }

    /// <summary>
    /// Get the tenant's profile information
    /// </summary>
    /// <returns>Tenant profile information</returns>
    [HttpGet("settings/profile")]
    public async Task<IActionResult> GetTenantProfile()
    {
        if (!HttpContext.User.Identity?.IsAuthenticated ?? true)
            return Unauthorized(new { error = "Unauthorized" });

        var tenantAccountId = HttpContext.GetUserId();
        var tenantProfile = await tenantPortalService.GetTenantProfile(tenantAccountId);

        if (tenantProfile == null)
            return NotFound("Tenant profile not found");

        return Ok(tenantProfile);
    }

    /// <summary>
    /// Edit the tenant's profile information
    /// </summary>
    /// <param name="editTenantProfile"></param>
    /// <returns>Success response</returns>
    [HttpPut("settings/profile")]
    public async Task<IActionResult> EditTenantProfile([FromBody] EditTenantProfile editTenantProfile)
    {
        if (!HttpContext.User.Identity?.IsAuthenticated ?? true)
            return Unauthorized(new { error = "Unauthorized" });

        var tenantAccountId = HttpContext.GetUserId();
        await tenantPortalService.EditTenantProfile(tenantAccountId, editTenantProfile);

        return Ok(new { message = "Profile updated successfully" });
    }

    /// <summary>
    /// Change the tenant's password
    /// </summary>
    /// <param name="request"></param>
    /// <returns>Success response</returns>
    [HttpPost("settings/change-password")]
    public async Task<IActionResult> ChangeTenantPassword([FromBody] ChangeTenantPasswordRequest request)
    {
        if (!HttpContext.User.Identity?.IsAuthenticated ?? true)
            return Unauthorized(new { error = "Unauthorized" });

        var tenantAccountId = HttpContext.GetUserId();
        try
        {
            await tenantPortalService.ChangeTenantPassword(tenantAccountId, request);
            return Ok(new { message = "Password changed successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("accounts/create-for-tenant")]
    public async Task<IActionResult> CreateTenantAccountForTenant([FromServices] PmsDbContext pmsDbContext, [FromBody] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Email is required.");

        // Find the tenant by email
        var tenant = await pmsDbContext.Tenants.FirstOrDefaultAsync(t => t.Email == email);
        if (tenant == null)
            return NotFound("Tenant not found.");

        // Check if a TenantAccount already exists for this tenant
        if (await pmsDbContext.TenantAccounts.AnyAsync(a => a.Email == email))
            return BadRequest("Tenant account with this email already exists.");

        // Create the TenantAccount
        var tenantAccount = new TenantAccount
        {
            Email = email,
            HashedPassword = "a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3",
            CreatedOn = DateTime.UtcNow,
            IsArchived = false,
            HasAccess = true,
            LastLogin = DateTime.UtcNow,
            TenantId = tenant.Id
        };

        pmsDbContext.TenantAccounts.Add(tenantAccount);
        await pmsDbContext.SaveChangesAsync();

        return Ok(new { message = "Tenant account created successfully." });
    }
}
