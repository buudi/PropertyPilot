using System.ComponentModel.DataAnnotations;

namespace PropertyPilot.Services.UserServices.Models;

public class UpdateUserRequest
{
    [Required]
    public required string Name { get; init; }

    [Required]
    public required string Email { get; init; }

    [Required]
    public required bool HasAccess { get; init; }

    [Required]
    public required string MonetaryAccountName { get; init; }

    public List<AssignCaretakerPropertyRequest>? CaretakerProperties { get; set; } = [];
}
