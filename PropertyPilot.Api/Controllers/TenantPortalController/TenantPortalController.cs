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
        var userId = HttpContext.GetUserId();

        var response = await tenantPortalService.GetBasicTenantInfo(userId);

        if (response == null)
        {
            return NotFound("Tenant Not Found");
        }

        return Ok(response);
    }

}
