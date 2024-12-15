using Microsoft.AspNetCore.Mvc;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.TenantsServices;
using PropertyPilot.Services.TenantsServices.Models;

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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<Tenant>> CreateTenant(CreateTenantRequest request)
    {
        var newTenant = await tenantsService.CreateTenantBasicInfo(request);

        //return Ok(newTenant);

        return CreatedAtAction(
            nameof(GetTenantById),
            new { id = newTenant.Id },
            newTenant);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="requests"></param>
    /// <returns></returns>
    [HttpPost("batch-create")]
    public async Task<ActionResult<List<Tenant>>> CreateTenants([FromBody] List<CreateTenantRequest> requests)
    {
        if (requests == null || !requests.Any())
        {
            return BadRequest("The list of tenant requests cannot be null or empty.");
        }

        var newTenants = await tenantsService.CreateTenantsBasicInfo(requests);

        return CreatedAtAction(
            nameof(GetAllTenants),
            null,
            newTenants);
    }
}
