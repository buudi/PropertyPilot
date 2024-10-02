using System.ComponentModel.DataAnnotations;

namespace PropertyPilot.Services.UserServices.Models;

public class BaseUserRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;
}
