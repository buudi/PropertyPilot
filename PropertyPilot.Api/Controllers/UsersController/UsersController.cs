using Microsoft.AspNetCore.Mvc;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.UserServices;
using PropertyPilot.Services.UserServices.Models;

namespace PropertyPilot.Api.Controllers.UsersController;

[Route("api/[controller]")]
[ApiController]
public class UsersController(UserService userService) : ControllerBase
{
    [HttpPost]
    public ActionResult<PropertyPilotUser> CreateUser(CreateUserRequest request)
    {
        var createdUser = userService.CreateUser(request);

        return createdUser;
    }
}
