using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyPilot.Dal.Models;

[Table("contracts")]
public class Contract
{

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    public Guid TenantId { get; set; } // Foreign key for Tenant 

    [ForeignKey(nameof(TenantId))]
    public required Tenant Tenant { get; set; }

    [Required]
    public Guid PropertyId { get; set; } // Foreign key for Property

    [ForeignKey(nameof(PropertyId))]
    public required Property Property { get; set; }

    [Required]
    public required DateTime StartDate { get; set; }

    [Required]
    public required DateTime EndDate { get; set; }

    [Required]
    public required double Rent { get; set; }

    public string? Notes { get; set; }

    [Required]
    public required bool Active { get; set; }

    [Required]
    public required bool Renewable { get; set; }

    [Required]
    public required bool MoveOut { get; set; }
}
