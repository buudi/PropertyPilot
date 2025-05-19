using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyPilot.Api.Constants;
using PropertyPilot.Api.Extensions;
using PropertyPilot.Services.CaretakerPortalServices;
using PropertyPilot.Services.CaretakerPortalServices.Models.Finances;
using PropertyPilot.Services.CaretakerPortalServices.Models.Settings;
using PropertyPilot.Services.FinanceServices.Models;
using PropertyPilot.Services.TenantServices;
using PropertyPilot.Services.TenantServices.Models;

namespace PropertyPilot.Api.Controllers.CaretakerPortalController;

/// <summary>
/// PropertyPilot Caretaker Portal API
/// </summary>
[Route("api/caretaker-portal")]
[ApiController]
public class CaretakerPortalController(CaretakerPortalService caretakerPortalService, TenantService tenantService) : ControllerBase
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = AuthPolicies.CaretakerOnly)]
    [HttpGet("home-screen")]
    public async Task<IActionResult> GetHomeScreen()
    {
        var userId = HttpContext.GetUserId();
        var homeScreen = await caretakerPortalService.CaretakerPortalHomeScreen(userId);

        return Ok(homeScreen);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = AuthPolicies.CaretakerOnly)]
    [HttpGet("finances-screen")]
    public async Task<IActionResult> GetFinancesScreen([FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 1)
    {
        var userId = HttpContext.GetUserId();
        var financesScreen = await caretakerPortalService.CaretakerPortalFinancesScreen(userId, pageSize, pageNumber);

        return Ok(financesScreen);
    }

    /// <summary>
    /// Record a deposit for the caretaker
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = AuthPolicies.CaretakerOnly)]
    [HttpPost("finances/deposit")]
    public async Task<IActionResult> RecordDeposit([FromBody] RecordDepositRequest request)
    {
        var userId = HttpContext.GetUserId();
        var recordDepositAttempt = await caretakerPortalService.RecordDeposit(userId, request.Amount);

        if (recordDepositAttempt.IsSuccess == false)
        {
            return StatusCode(recordDepositAttempt.ErrorCode!.Value, new { message = recordDepositAttempt.ErrorMessage });
        }

        return Ok(recordDepositAttempt.Value);
    }

    /// <summary>
    /// Get the caretaker's profile information
    /// Name, email, phone number, member since
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = AuthPolicies.CaretakerOnly)]
    [HttpGet("settings/profile")]
    public async Task<IActionResult> GetCaretakerProfile()
    {
        var userId = HttpContext.GetUserId();
        var caretakerProfile = await caretakerPortalService.CaretakerPortalProfile(userId);

        return Ok(caretakerProfile);
    }

    /// <summary>
    /// Edit the caretaker's profile information
    /// </summary>
    /// <param name="editCaretakerProfile"></param>
    /// <returns></returns>
    [Authorize(Policy = AuthPolicies.CaretakerOnly)]
    [HttpPut("settings/profile")]
    public async Task<IActionResult> EditCaretakerProfile([FromBody] EditCaretakerProfile editCaretakerProfile)
    {
        var userId = HttpContext.GetUserId();
        await caretakerPortalService.EditCaretakerProfile(userId, editCaretakerProfile);

        return NoContent();
    }

    /// <summary>
    /// Get properties Tenant tab listing
    /// </summary>
    /// <param name="propertyId"></param>
    /// <returns></returns>
    [Authorize(Policy = AuthPolicies.CaretakerOnly)]
    [HttpGet("properties/{propertyId:guid}/tenants")]
    public async Task<IActionResult> GetPropertiesTenantTabListing([FromRoute] Guid propertyId)
    {
        var tenantTabListing = await caretakerPortalService.GetPropertiesTenantTabListing(propertyId);

        return Ok(tenantTabListing);
    }

    /// <summary>
    /// Add a new tenant
    /// </summary>
    /// <param name="tenantCreateRequest"></param>
    /// <returns></returns>
    [Authorize(Policy = AuthPolicies.CaretakerOnly)]
    [HttpPost("tenants")]
    public async Task<IActionResult> AddNewTenant([FromBody] TenantCreateRequest tenantCreateRequest)
    {
        var newTenant = await tenantService.CreateTenantAsync(tenantCreateRequest);

        return StatusCode(201, new { tenantId = newTenant.Id });
    }

    /// <summary>
    /// Get Invoices Tab Listing for property
    /// </summary>
    /// <param name="propertyId"></param>
    /// <param name="pageSize"></param>
    /// <param name="pageNumber"></param>
    /// <returns></returns>
    [Authorize(Policy = AuthPolicies.CaretakerOnly)]
    [HttpGet("properties/{propertyId:guid}/invoices")]
    public async Task<IActionResult> GetInvoicesTabListing(
        [FromRoute] Guid propertyId,
        [FromQuery] int pageSize = 10,
        [FromQuery] int pageNumber = 1)
    {
        if (pageSize <= 0 || pageNumber <= 0)
        {
            return BadRequest("PageSize and PageNumber must be greater than zero.");
        }

        var result = await caretakerPortalService.GetPropertiesInvoicesTabAsync(propertyId, pageSize, pageNumber);

        return Ok(result);

    }

    /// <summary>
    /// Get Payments Tab Listing
    /// </summary>
    /// <param name="propertyId"></param>
    /// <param name="pageSize"></param>
    /// <param name="pageNumber"></param>
    /// <returns></returns>
    [Authorize(Policy = AuthPolicies.CaretakerOnly)]
    [HttpGet("properties/{propertyId:guid}/payments")]
    public async Task<IActionResult> GetPaymentsTabListing(
        [FromRoute] Guid propertyId,
        [FromQuery] int pageSize = 10,
        [FromQuery] int pageNumber = 1)
    {
        if (pageSize <= 0 || pageNumber <= 0)
        {
            return BadRequest("PageSize and PageNumber must be greater than zero.");
        }

        var result = await caretakerPortalService.GetPaymentsTabAsync(propertyId, pageSize, pageNumber);

        return Ok(result);

    }

    /// <summary>
    /// Get Expenses Tab Listing
    /// </summary>
    /// <param name="propertyId"></param>
    /// <param name="pageSize"></param>
    /// <param name="pageNumber"></param>
    /// <returns></returns>
    [Authorize(Policy = AuthPolicies.CaretakerOnly)]
    [HttpGet("properties/{propertyId:guid}/expenses")]
    public async Task<IActionResult> GetExpensesTabListing(
        [FromRoute] Guid propertyId,
        [FromQuery] int pageSize = 10,
        [FromQuery] int pageNumber = 1)
    {
        if (pageSize <= 0 || pageNumber <= 0)
        {
            return BadRequest("PageSize and PageNumber must be greater than zero.");
        }

        var result = await caretakerPortalService.GetExpenseTabListing(propertyId, pageSize, pageNumber);

        return Ok(result);

    }

    /// <summary>
    /// Get Sub Units Tab Listing
    /// </summary>
    /// <param name="propertyId"></param>
    /// <returns></returns>
    [Authorize(Policy = AuthPolicies.CaretakerOnly)]
    [HttpGet("properties/{propertyId:guid}/sub-units")]
    public async Task<IActionResult> GetSubunitsTabListing([FromRoute] Guid propertyId)
    {
        var result = await caretakerPortalService.GetSubunitsTab(propertyId);
        return Ok(result);
    }

    /// <summary>
    /// record rent payment
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = AuthPolicies.CaretakerOnly)]
    [HttpPost("finances/record-rent")]
    public async Task<IActionResult> RecordRentPayment([FromBody] RentPaymentRequest request)
    {
        var userId = HttpContext.GetUserId();

        var rentPaymentResult = await caretakerPortalService.RecordRentPayment(userId, request);

        if (rentPaymentResult.IsSuccess)
        {
            return StatusCode(201);
        }

        switch (rentPaymentResult.ErrorCode)
        {
            case 400:
                return BadRequest(new { message = rentPaymentResult.ErrorMessage });
            case 404:
                return NotFound(new { message = rentPaymentResult.ErrorMessage });
            case 409:
                return Conflict(new { message = rentPaymentResult.ErrorMessage });
            default:
                return StatusCode(rentPaymentResult.ErrorCode!.Value, new { message = rentPaymentResult.ErrorMessage });
        }

    }

}


