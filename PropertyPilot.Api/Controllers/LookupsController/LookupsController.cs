using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyPilot.Services.Generics;
using PropertyPilot.Services.LookupServices;
using PropertyPilot.Services.LookupServices.Models;

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
        var response = new ItemsResponse<List<PropertyListingsLookup>>(lookups);
        return Ok(response);
    }
}
