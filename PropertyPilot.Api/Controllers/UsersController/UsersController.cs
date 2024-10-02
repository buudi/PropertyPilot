using Microsoft.AspNetCore.Mvc;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.UserServices;
using PropertyPilot.Services.UserServices.Models;

namespace PropertyPilot.Api.Controllers.UsersController;

[Route("api/[controller]")]
[ApiController]
public class UsersController(UserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PropertyPilotUser>>> GetAllUsers()
    {
        var users = await userService.GetAllUsersAsync();
        return users;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PropertyPilotUser>> GetUserById(Guid id)
    {
        var user = await userService.GetUserById(id);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPost]
    public ActionResult<PropertyPilotUser> CreateUser(CreateUserRequest request)
    {
        var createdUser = userService.CreateUser(request);

        return CreatedAtAction(
            nameof(GetUserById),
            new { id = createdUser.Id },
            createdUser);
    }
}
