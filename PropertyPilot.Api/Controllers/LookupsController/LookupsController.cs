using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyPilot.Services.LookupServices;

namespace PropertyPilot.Api.Controllers.LookupsController;


/// <summary>
/// Lookups Controller: the API's sole purpose is to provide data for dropdowns
/// </summary>
[Route("api/lookups")]
[ApiController]
public class LookupsController(LookupService lookupService) : ControllerBase
{

    /// <summary>
    /// listing of properties IDs, property name, their type and the sub-units count if any
    /// </summary>
    /// <returns>List<PropetyListingsLookup></returns>
    [Authorize(Policy = "ManagerAndAbove")]
    [HttpGet("property-listings")]
    public async Task<IActionResult> GetPropertyListingsLookup()
    {
        var lookups = await lookupService.PropertyListingsLookup();
        return Ok(lookups);
    }
}
