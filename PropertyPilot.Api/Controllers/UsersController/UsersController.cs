using Microsoft.AspNetCore.Mvc;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.UserServices;
using PropertyPilot.Services.UserServices.Models;

namespace PropertyPilot.Api.Controllers.UsersController;


/// <summary>
/// PropertyPilot Users API
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class UsersController(UserService userService) : ControllerBase
{
    /// <summary>
    /// Get all PropertyPilot users
    /// </summary>
    /// <returns>List of all PropertyPilot users.</returns>
    [HttpGet]
    public async Task<ActionResult<List<PropertyPilotUser>>> GetAllUsers()
    {
        var users = await userService.GetAllUsersAsync();
        return users;
    }

    /// <summary>
    /// Get PropertyPilot user by Id
    /// </summary>
    /// <returns>PropertyPilot user object.</returns>
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

    /// <summary>
    /// Create PropertyPilot user
    /// </summary>
    /// <param name="request"></param>
    /// <returns>Created PropertyPilot User</returns>
    [HttpPost]
    public ActionResult<PropertyPilotUser> CreateUser(CreateUserRequest request)
    {
        var createdUser = userService.CreateUser(request);

        return CreatedAtAction(
            nameof(GetUserById),
            new { id = createdUser.Id },
            createdUser);
    }

    /// <summary>
    /// Updates user Email and Name
    /// </summary>
    [HttpPut("{id:guid}")]
    public IActionResult UpdateUser(Guid id, UpdateUserRequest request)
    {
        userService.UpdateUser(id, request);

        return NoContent();
    }
}
