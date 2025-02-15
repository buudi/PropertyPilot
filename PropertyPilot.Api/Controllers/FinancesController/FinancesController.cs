using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyPilot.Api.Constants;
using PropertyPilot.Services.FinanceServices;
using PropertyPilot.Services.FinanceServices.Models;
using System.Security.Claims;

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

    /// <summary>
    /// Retrieves a paginated list of monetary accounts.
    /// </summary>
    /// <param name="pageSize">
    /// The number of items to include in each page. Must be greater than zero.
    /// </param>
    /// <param name="pageNumber">
    /// The page number to retrieve. Must be greater than zero.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a paginated list of monetary accounts.
    /// </returns>
    /// <remarks>
    /// This endpoint requires the "ManagerAndAbove" authorization policy.
    /// </remarks>
    [Authorize(Policy = AuthPolicies.ManagerAndAbove)]
    [HttpGet("accounts/listings")]
    public async Task<IActionResult> GetAllMonetaryAccountsListingItems(
        [FromQuery] int pageSize = 10,
        [FromQuery] int pageNumber = 1)
    {
        if (pageSize <= 0 || pageNumber <= 0)
        {
            return BadRequest("PageSize and PageNumber must be greater than zero.");
        }

        var result = await financesService.GetAllMonetaryAccountsListingItems(pageSize, pageNumber);

        return Ok(result);
    }


    /// <summary>
    /// Retrieves the rent payment transaction records associated with a specific invoice.
    /// </summary>
    /// <param name="invoiceId">
    /// The unique identifier of the invoice for which the rent payment transaction records are to be retrieved.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the rent payment transaction records for the specified invoice.
    /// </returns>
    /// <remarks>
    /// This endpoint requires the user to have the "ManagerAndAbove" authorization policy.
    /// </remarks>
    [Authorize(Policy = AuthPolicies.AllRoles)]
    [HttpGet("rent-payments/invoice")]
    public async Task<IActionResult> GetRentPaymentTransactionRecordForInvoice(Guid invoiceId)
    {
        var record = await financesService.GetRentPaymentTransactionRecordForInvoice(invoiceId);

        return Ok(record);
    }


    /// <summary>
    /// Retrieves the transaction record for a specific rent payment.
    /// </summary>
    /// <param name="rentPaymentId">
    /// The unique identifier of the rent payment whose transaction record is to be retrieved.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the transaction record if found, or a <see cref="NotFoundResult"/> if no record exists for the specified rent payment.
    /// </returns>
    /// <remarks>
    /// This endpoint requires the user to have the "ManagerAndAbove" authorization policy.
    /// </remarks>
    [Authorize(Policy = AuthPolicies.AllRoles)]
    [HttpGet("rent-payments")]
    public async Task<IActionResult> GetRentPaymentTransactionRecord(Guid rentPaymentId)
    {
        var record = await financesService.GetRentPaymentTransactionRecordByPaymentId(rentPaymentId);

        if (record == null)
        {
            return NotFound();
        }

        return Ok(record);
    }

    /// <summary>
    /// Creates a new rent payment request.
    /// </summary>
    /// <param name="request">
    /// The <see cref="RentPaymentRequest"/> containing details of the rent payment to be created, 
    /// including tenant ID, invoice ID, amount, and payment method.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> representing the result of the operation:
    /// <list type="bullet">
    /// <item><description>Returns <see cref="CreatedAtActionResult"/> with the created rent payment details if successful.</description></item>
    /// <item><description>Returns <see cref="NotFoundResult"/> if the specified resources are not found.</description></item>
    /// <item><description>Returns <see cref="ConflictResult"/> if there is a conflict in the request.</description></item>
    /// <item><description>Returns <see cref="BadRequestResult"/> if the request is invalid.</description></item>
    /// <item><description>Returns a custom status code with an error message for other error scenarios.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This endpoint is restricted to users with the "ManagerAndAbove" policy.
    ///
    /// #### Error Codes
    /// - **400 Bad Request**: The request contains invalid data.
    /// - **404 Not Found**: The specified tenant or invoice does not exist.
    /// - **409 Conflict**: A conflicting rent payment request already exists.
    /// - **500 Internal Server Error**: An unexpected error occurred.
    /// </remarks>

    [Authorize(Policy = AuthPolicies.AllRoles)]
    [HttpPost("rent-payments")]
    public async Task<IActionResult> CreateRentPaymentRequest(RentPaymentRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        var userGuid = Guid.Parse(userId);

        var rentPaymentResult = await financesService.RecordRentPayment(userGuid, request);

        if (rentPaymentResult.IsSuccess)
        {
            return CreatedAtAction(nameof(GetRentPaymentTransactionRecord),
                new
                {
                    rentPaymentId = rentPaymentResult.Value.RentPayment.Id
                },
                rentPaymentResult.Value.RentPayment
            );
        }

        switch (rentPaymentResult.ErrorCode)
        {
            case 404:
                return NotFound(new { message = rentPaymentResult.ErrorMessage });
            case 409:
                return Conflict(new { message = rentPaymentResult.ErrorMessage });
            case 400:
                return BadRequest(new { message = rentPaymentResult.ErrorMessage });
            default:
                return StatusCode(rentPaymentResult.ErrorCode.Value, new { message = rentPaymentResult.ErrorMessage });
        }
    }

    /// <summary>
    /// get transactions listing
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>

    [Authorize(Policy = AuthPolicies.ManagerAndAbove)]
    [HttpGet("transactions/listings")]
    public async Task<IActionResult> GetTransactionsListings([FromQuery] int pageNumber = 1, int pageSize = 10)
    {
        var listings = await financesService.GetTransactionsListingsAsync(pageNumber, pageSize);

        return Ok(listings);
    }
}