using System.ComponentModel.DataAnnotations;

namespace PropertyPilot.Services.UserServices.Models;

public class BaseUserRequest
{
    [Required]
    public required string Name { get; set; }

    [Required]
    public required string Email { get; set; }

    [Required]
    public required string Role { get; set; }

    [Required]
    public bool Access { get; set; }
}
