using System.ComponentModel.DataAnnotations;

namespace PropertyPilot.Services.TenantPortalServices.Models.Settings;

public class EditTenantProfile
{
    [Required]
    public required string Name { get; set; }

    [Required]
    public required string PhoneNumber { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }
} 