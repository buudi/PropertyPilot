using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyPilot.Api.Constants;
using PropertyPilot.Services.InvoiceServices;

namespace PropertyPilot.Api.Controllers.InvoicesController;

/// <summary>
/// Provides API endpoints for managing invoices, including retrieving paginated invoice listings.
/// </summary>
/// <remarks>
/// This controller is responsible for handling HTTP requests related to invoices.
/// It includes endpoints that require specific authorization policies, such as "ManagerAndAbove".
/// </remarks>
/// <param name="invoicesService">
/// The service responsible for performing operations related to invoices.
/// </param>
[ApiController]
[Route("api/invoices")]
public class InvoicesController(InvoicesService invoicesService) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of invoice listing items within a specified date range.
    /// </summary>
    /// <param name="pageSize">The number of items to include on each page. Must be greater than zero.</param>
    /// <param name="pageNumber">The page number to retrieve. Must be greater than zero.</param>
    /// <param name="createDateFrom">The start date of the invoice creation date range.</param>
    /// <param name="createDateTill">The end date of the invoice creation date range.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a paginated list of invoice listing items if successful,
    /// or a <see cref="BadRequestObjectResult"/> if the input parameters are invalid.
    /// </returns>
    /// <remarks>
    /// This endpoint is restricted to users with the "ManagerAndAbove" authorization policy.
    /// </remarks>
    [Authorize(Policy = AuthPolicies.ManagerAndAbove)]
    [HttpGet("listings")]
    public async Task<IActionResult> GetAllInvoicesListingItems(
        [FromQuery] int pageSize = 10,
        [FromQuery] int pageNumber = 1,
        [FromQuery] DateTime? createDateFrom = null,
        [FromQuery] DateTime? createDateTill = null)
    {
        // Ensure valid pagination values
        if (pageSize <= 0 || pageNumber <= 0)
        {
            return BadRequest("PageSize and PageNumber must be greater than zero.");
        }

        // default date range to the last 30 days if not provided
        var now = DateTime.UtcNow;
        var fromDate = createDateFrom ?? now.AddDays(-30); // last 30 days  
        var tillDate = createDateTill ?? now; // up to today

        if (fromDate > tillDate)
        {
            return BadRequest("CreateDateFrom cannot be greater than CreateDateTill.");
        }

        var result = await invoicesService.GetAllInvoicesListingItems(pageSize, pageNumber, fromDate, tillDate);

        return Ok(result);
    }


}