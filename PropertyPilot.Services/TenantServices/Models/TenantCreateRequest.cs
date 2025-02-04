using System.ComponentModel.DataAnnotations;

namespace PropertyPilot.Services.TenantServices.Models;

public record TenantCreateRequest
{
    [Required]
    public required string Name { get; set; }

    [Required]
    public required string PhoneNumber { get; set; }

    [Required]
    public required string Email { get; set; }

    [Required]
    public required string TenantIdentification { get; set; }
}
