using Microsoft.AspNetCore.Mvc;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.TenantsServices;

namespace PropertyPilot.Api.Controllers.TenantsController;

/// <summary>
/// 
/// </summary>
[Route("api/tenants")]
[ApiController]
public class TenantsController(TenantsService tenantsService) : ControllerBase
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<List<Tenant>> GetAllTenants()
    {
        var tenants = await tenantsService.GetAllTenantsAsync();
        return tenants;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Tenant?>> GetTenantById(Guid id)
    {
        var tenant = await tenantsService.GetTenantByIdAsync(id);

        if (tenant == null)
        {
            return NotFound();
        }

        return Ok(tenant);
    }
}
