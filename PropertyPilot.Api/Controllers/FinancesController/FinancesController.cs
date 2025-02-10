using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyPilot.Api.Constants;
using PropertyPilot.Services.FinanceServices;

namespace PropertyPilot.Api.Controllers.FinancesController;

/// <summary>
/// Provides API endpoints for managing invoices, including retrieving paginated invoice listings.
/// </summary>
/// <remarks>
/// This controller is responsible for handling HTTP requests related to invoices.
/// It includes endpoints that require specific authorization policies, such as "ManagerAndAbove".
/// </remarks>
/// <param name="financesService">
/// The service responsible for performing operations related to invoices.
/// </param>
[ApiController]
[Route("api/finances")]
public class FinancesController(FinancesService financesService) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of invoice listing items within a specified date range.
    /// </summary>
    /// <param name="pageSize">
    /// The number of items to include in each page. Defaults to 10. Must be greater than zero.
    /// </param>
    /// <param name="pageNumber">
    /// The page number to retrieve. Defaults to 1. Must be greater than zero.
    /// </param>
    /// <param name="createDateFrom">
    /// The start date of the invoice creation date range. Defaults to 30 days prior to the current date if not provided.
    /// </param>
    /// <param name="createDateTill">
    /// The end date of the invoice creation date range. Defaults to the current date if not provided.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a paginated list of invoice listing items or a bad request response
    /// if the input parameters are invalid.
    /// </returns>
    /// <remarks>
    /// This endpoint is restricted to users with the "ManagerAndAbove" authorization policy.
    /// </remarks>
    [Authorize(Policy = AuthPolicies.ManagerAndAbove)]
    [HttpGet("invoices/listings")]
    public async Task<IActionResult> GetAllInvoicesListingItems(
        [FromQuery] int pageSize = 10,
        [FromQuery] int pageNumber = 1,
        [FromQuery] DateTime? createDateFrom = null,
        [FromQuery] DateTime? createDateTill = null)
    {

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

        var result = await financesService.GetAllInvoicesListingItems(pageSize, pageNumber, fromDate, tillDate);

        return Ok(result);
    }


}