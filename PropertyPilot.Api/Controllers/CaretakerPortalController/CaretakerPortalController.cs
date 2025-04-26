using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyPilot.Api.Constants;
using PropertyPilot.Api.Extensions;
using PropertyPilot.Services.CaretakerPortalServices;

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
}
