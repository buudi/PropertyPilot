using System.ComponentModel.DataAnnotations;

namespace PropertyPilot.Services.UserServices.Models;

public class CreateUserRequest : BaseUserRequest
{
    [Required]
    public string HashedPassword { get; set; } = string.Empty;
}
