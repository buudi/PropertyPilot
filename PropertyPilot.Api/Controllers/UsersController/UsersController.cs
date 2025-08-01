﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyPilot.Api.Constants;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.UserServices;
using PropertyPilot.Services.UserServices.Models;

namespace PropertyPilot.Api.Controllers.UsersController;


/// <summary>
/// PropertyPilot Users API
/// </summary>
[Route("api/users")]
[ApiController]
public class UsersController(UserService userService) : ControllerBase
{
    /// <summary>
    /// Get all PropertyPilot users
    /// Policy: Admin Manager Only
    /// </summary>
    /// <returns>List of all PropertyPilot users.</returns>
    [Authorize(Policy = AuthPolicies.ManagerAndAbove)]
    [HttpGet]
    public async Task<IActionResult> GetAllUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var paginatedUsers = await userService.GetAllUsersAsync(pageNumber, pageSize);
        return Ok(paginatedUsers);
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
    public async Task<ActionResult<PropertyPilotUser>> CreateUser(CreateUserRequest request)
    {
        var createdUser = await userService.CreateUser(request);

        return CreatedAtAction(
            nameof(GetUserById),
            new { id = createdUser.Id },
            createdUser);
    }

    /// <summary>
    /// Updates user Email and Name
    /// </summary>
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, UpdateUserRequest request)
    {
        await userService.UpdateUser(id, request);

        return NoContent();
    }
}
