using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyPilot.Api.Constants;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.Generics;
using PropertyPilot.Services.PropertiesServices;
using PropertyPilot.Services.PropertiesServices.Models;
using PropertyPilot.Services.PropertyListingServices.Models;

namespace PropertyPilot.Api.PropertiesConrtoller;

/// <summary>
/// 
/// </summary>
/// <param name="propertiesService"></param>
[Authorize(Policy = AuthPolicies.ManagerAndAbove)]
[Route("api/properties")]
[ApiController]
public class PropertiesController(PropertiesService propertiesService) : ControllerBase
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> GetAllProperties([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var propertyListings = await propertiesService.GetAllPropertyAsync(pageNumber, pageSize);
        return Ok(propertyListings);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet("dashboard")]
    public IActionResult GetPropertiesDashboard()
    {
        var propertyDashboard = propertiesService.GetPropertiesDashboard();
        return Ok(propertyDashboard);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPropertyById(Guid id)
    {
        var properties = await propertiesService.GetPropertyByIdAsync(id);

        if (properties == null)
        {
            return NotFound();
        }

        return Ok(properties);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="createPropertyRequest"></param>
    /// <returns></returns>
    [HttpPost]
    public ActionResult<Property> CreateProperty([FromBody] CreatePropertyRequest createPropertyRequest)
    {
        var newProperty = propertiesService.CreateProperty(createPropertyRequest);

        return CreatedAtAction(
            nameof(GetPropertyById),
            new { id = newProperty.Id },
            newProperty);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdatePropertyAsync(Guid id, [FromBody] UpdatePropertyRequest request)
    {
        await propertiesService.UpdatePropertyAsync(id, request);

        return NoContent();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="createPropertyRequests"></param>
    /// <returns></returns>
    [HttpPost("batch-add")]
    public ActionResult<List<Property>> CreateProperties([FromBody] List<CreatePropertyRequest> createPropertyRequests)
    {
        try
        {
            var newProperties = propertiesService.CreateProperties(createPropertyRequests);

            return CreatedAtAction(
                nameof(GetAllProperties),
                null,
                newProperties);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// get tenants timeline for give property unit
    /// </summary>
    /// <param name="propertyId"></param>
    /// <returns></returns>
    [HttpGet("{propertyId:guid}/tenants/timeline")]
    public async Task<IActionResult> TenantsTimeline([FromRoute] Guid propertyId)
    {
        var timeline = await propertiesService.GetPropertyTenantsTimelineAsync(propertyId);
        var response = new ItemsResponse<TimelineResponse>(timeline);
        return Ok(response);
    }
}
