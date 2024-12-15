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
    public required Guid TenantId { get; set; } // Foreign key for Tenant 

    [Required]
    public required Guid PropertyId { get; set; } // Foreign key for Property

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

    [Required]
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(TenantId))]
    public Tenant Tenant { get; set; } = null!; // navigation property to associated tenant
    [ForeignKey(nameof(PropertyId))]
    public Property Property { get; set; } = null!; // navigation property to associated property
}
