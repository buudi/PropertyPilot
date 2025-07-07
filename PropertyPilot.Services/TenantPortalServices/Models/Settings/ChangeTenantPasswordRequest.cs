using System.ComponentModel.DataAnnotations;

namespace PropertyPilot.Services.TenantPortalServices.Models.Settings;

public class ChangeTenantPasswordRequest
{
    [Required]
    public required string CurrentPassword { get; set; }

    [Required]
    [MinLength(6)]
    public required string NewPassword { get; set; }
} 