using System.ComponentModel.DataAnnotations;

namespace PropertyPilot.Api.Controllers.UsersController.Models;

public class CreateUserRequest : BaseUserRequest
{
    [Required]
    public string HashedPassword { get; set; } = string.Empty;
}
