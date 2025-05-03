using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyPilot.Api.Constants;
using PropertyPilot.Api.Extensions;
using PropertyPilot.Services.CaretakerPortalServices;
using PropertyPilot.Services.CaretakerPortalServices.Models.Finances;

namespace PropertyPilot.Api.Controllers.CaretakerPortalController;

/// <summary>
/// PropertyPilot Caretaker Portal API
/// </summary>
[Route("api/caretaker-portal")]
[ApiController]
public class CaretakerPortalController(CaretakerPortalService caretakerPortalService) : ControllerBase
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


}
