using System.ComponentModel.DataAnnotations;

namespace PropertyPilot.Services.UserServices.Models;

public class CreateUserRequest : BaseUserRequest
{
    [Required]
    public string Password { get; set; } = string.Empty;
}
