using Microsoft.AspNetCore.Mvc;
using PropertyPilot.Dal.Models;

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
