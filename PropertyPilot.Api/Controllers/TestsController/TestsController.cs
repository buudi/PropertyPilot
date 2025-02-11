using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyPilot.Api.Constants;
using PropertyPilot.Services.FinanceServices;

namespace PropertyPilot.Api.Controllers.TestsController;

/// <summary>
/// Provides API endpoints for managing and testing functionalities.
/// </summary>
/// <remarks>
/// This controller is restricted to users with the "AdminManagerOnly" policy.
/// </remarks>
[Authorize(Policy = AuthPolicies.AdminManagerOnly)]
[Route("api/tests")]
[ApiController]
public class TestsController(FinancesService financesService) : ControllerBase
{
    /// <summary>
    /// Ensures the creation of the main monetary account in the system.
    /// </summary>
    /// <remarks>
    /// This method invokes the <see cref="FinancesService.CreateMainMonetaryAccount"/> method to check for the existence of 
    /// a monetary account named "Main" and creates it if it does not exist.
    /// </remarks>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
    [HttpPost("finances/accounts/main-account")]
    public async Task<IActionResult> CreateMainMonetaryAccount()
    {
        await financesService.CreateMainMonetaryAccount();
        return Ok();
    }

}