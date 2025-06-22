using Microsoft.AspNetCore.Mvc;
using PropertyPilot.Api.Extensions;
using PropertyPilot.Services.TenantPortalServices;

namespace PropertyPilot.Api.Controllers.TenantPortalController;

[Route("api/tenants-portal")]
[ApiController]
public class TenantPortalController(TenantPortalService tenantPortalService) : ControllerBase
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
}
