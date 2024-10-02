using System.ComponentModel.DataAnnotations;

namespace PropertyPilot.Services.UserServices.Models;

public class UpdateUserRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;
}
