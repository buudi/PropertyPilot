using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyPilot.Api.Constants;
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
    /// <returns>List of PropetyListingsLookup</returns>
    /// <remarks>listing of properties IDs, property name, their type and the sub-units count if any</remarks>
    [Authorize(Policy = AuthPolicies.ManagerAndAbove)]
    [HttpGet("property-listings")]
    public async Task<IActionResult> GetPropertyListingsLookup()
    {
        var lookups = await lookupService.PropertyListingsLookup();
        var response = new ItemsResponse<List<PropertyListingsLookup>>(lookups);
        return Ok(response);
    }


    /// <summary>
    /// monetary accounts IDs, account name, and the account balance
    /// </summary>
    /// <returns>List of MonetaryAccountLookup</returns>
    /// <remarks>monetary accounts IDs, account name, and the account balance</remarks>
    [Authorize(Policy = AuthPolicies.AllRoles)]
    [HttpGet("monetary-accounts")]
    public async Task<IActionResult> GetMonetaryAccountsLookup()
    {
        var lookups = await lookupService.MonetaryAccountLookup();
        var response = new ItemsResponse<List<MonetaryAccountLookup>>(lookups);
        return Ok(response);
    }

    /// <summary>
    /// Tenants IDs, and their name
    /// </summary>
    /// <returns>List Of TenantLookup</returns>
    /// <remarks>Tenants IDs, and their name</remarks>
    [Authorize(Policy = AuthPolicies.AllRoles)]
    [HttpGet("tenants")]
    public async Task<IActionResult> GetTenantsLookup()
    {
        var lookups = await lookupService.TenantLookup();
        var response = new ItemsResponse<List<TenantLookup>>(lookups);
        return Ok(response);
    }


    /// <summary>
    /// reuturn unpaid invoices (pending and outstanding) for given tenant, returns Invoice IDs, Date Start, Invoice Status and Amount Remaining
    /// </summary>
    /// <returns>List of InvoiceLookup</returns>
    /// <remarks>reuturn unpaid invoices (pending and outstanding) for given tenant, returns Invoice IDs, Date Start, Invoice Status and Amount Remaining</remarks>
    [HttpGet("tenants/invoices/{tenantId:Guid}")]
    public async Task<IActionResult> GetInvoicesForTenantLookup([FromRoute] Guid tenantId)
    {
        var lookups = await lookupService.InvoiceLookupForTenant(tenantId);
        var response = new ItemsResponse<List<InvoiceLookup>>(lookups);
        return Ok(response);
    }
}
