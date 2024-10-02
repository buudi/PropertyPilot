using System.ComponentModel.DataAnnotations;

namespace PropertyPilot.Api.Controllers.UsersController.Models;

public class BaseUserRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;
}
