using PropertyPilot.Dal.Abstractions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyPilot.Dal.Models;

[Table(nameof(Tenant))]
public class Tenant : ITenant
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string PhoneNumber { get; set; }

    public required string Email { get; set; }

    public required string TenantIdentification { get; set; }

    public bool IsLeavingThisMonth { get; set; } = false;

    public bool IsAccountActive { get; set; } = true;
}
