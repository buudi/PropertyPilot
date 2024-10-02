using Microsoft.AspNetCore.Mvc;
using PropertyPilot.Api.Controllers.UsersController.Models;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.UserServices;

namespace PropertyPilot.Api.Controllers.UsersController;

[Route("api/[controller]")]
[ApiController]
public class UsersController(UserService userService) : ControllerBase
{
    [HttpPost]
    public ActionResult<PropertyPilotUser> CreateUser(CreateUserRequest request)
    {
        userService.CreateUser()
    }
}
