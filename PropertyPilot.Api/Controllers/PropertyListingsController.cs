using Microsoft.AspNetCore.Mvc;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.PropertyListingServices;
using PropertyPilot.Services.PropertyListingServices.Models;

namespace PropertyPilot.Api.Controllers;

/// <summary>
/// 
/// </summary>
/// <param name="propertyListingService"></param>
[Route("api/properties-list")]
[ApiController]
public class PropertiesListController(PropertiesService propertyListingService) : ControllerBase
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<List<Property>> GetAllPropertyListingsAsync()
    {
        var listings = await propertyListingService.GetAllPropertyListingsAsync();
        return listings;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Property?>> GetPropertyListingById(Guid id)
    {
        var listing = await propertyListingService.GetPropertyListingByIdAsync(id);

        if (listing == null)
        {
            return NotFound();
        }

        return Ok(listing);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="createListingrequest"></param>
    /// <returns></returns>
    [HttpPost]
    public ActionResult<Property> CreatePropertyListing([FromBody] CreatePropertyListingRequest createListingrequest)
    {
        var newPropertyListing = propertyListingService.CreatePropertyListing(createListingrequest);

        return CreatedAtAction(
            nameof(GetPropertyListingById),
            new { id = newPropertyListing.Id },
            newPropertyListing);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdatePropertyListingAsync(Guid id, [FromBody] UpdatePropertyListingRequest request)
    {
        await propertyListingService.UpdatePropertyListingAsync(id, request);

        return NoContent();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
}
