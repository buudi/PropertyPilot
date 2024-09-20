using Microsoft.AspNetCore.Mvc;
using PropertyPilot.Api.Services.PropertyListingServices;
using PropertyPilot.Api.Services.PropertyListingServices.Models;
using PropertyPilot.Dal.Models;


namespace PropertyPilot.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PropertyListingsController(PropertyListingService propertyListingService) : ControllerBase
{
    [HttpGet]
    public async Task<List<PropertyListing>> GetAllPropertyListings()
    {
        List<PropertyListing> listings = await propertyListingService.GetAllPropertyListingsAsync();
        return listings;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PropertyListing?>> GetPropertyListingById(Guid id)
    {
        PropertyListing? listing = await propertyListingService.GetPropertyListingByIdAsync(id);

        return Ok(listing ?? null);
    }

    [HttpPost]
    public ActionResult<PropertyListing> CreatePropertyListing([FromBody] CreatePropertyListingRequest createListingrequest)
    {
        PropertyListing newPropertyListing = propertyListingService.CreatePropertyListing(createListingrequest);

        return CreatedAtAction(
            nameof(GetPropertyListingById),
            new { id = newPropertyListing.Id },
            newPropertyListing);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdatePropertyListingAsync(Guid id, [FromBody] UpdatePropertyListingRequest request)
    {
        await propertyListingService.UpdatePropertyListingAsync(id, request);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
}
