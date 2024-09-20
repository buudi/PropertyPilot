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

        return Ok(newPropertyListing);
    }

    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
}
