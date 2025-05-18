using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyPilot.Api.Constants;
using PropertyPilot.Services.TenantServices;
using PropertyPilot.Services.TenantServices.Models;

namespace PropertyPilot.Api.Controllers.TenantsController;

/// <summary>
/// Tenants Controller
/// </summary>

[Authorize(Policy = AuthPolicies.ManagerAndAbove)]
[Route("api/tenants")]
[ApiController]
public class TenantsController(TenantService tenantService) : ControllerBase
{

    /// <summary>
    /// get all tenants listing
    /// </summary>
    /// <returns>list of tenant records</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllTenants([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var tenants = await tenantService.GetAllTenantsListingAsync(pageNumber, pageSize);

        return Ok(tenants);
    }

    /// <summary>
    /// Get Tenant Listing Record By Tenant Id
    /// </summary>
    /// <param name="tenantId">Tenant UUID</param>
    /// <returns>Tenant Listing Record</returns>
    [HttpGet("{tenantId:guid}")]
    public async Task<IActionResult> GetTenantById(Guid tenantId)
    {
        var tenant = await tenantService.GetTenantRecordAsync(tenantId);

        if (tenant == null)
        {
            return NotFound();
        }

        return Ok(tenant);
    }

    /// <summary>
    /// Create Tenant id
    /// </summary>
    /// <param name="tenantCreateRequest"></param>
    /// <returns></returns>
    [Authorize(Policy = AuthPolicies.AllRoles)]
    [HttpPost]
    public async Task<IActionResult> CreateTenant(TenantCreateRequest tenantCreateRequest)
    {
        var newTenant = await tenantService.CreateTenantAsync(tenantCreateRequest);
        return CreatedAtAction(nameof(GetTenantById), new { tenantId = newTenant.Id }, newTenant);
    }
}
