using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyPilot.Dal.Models;

[Table(nameof(Tenancy))]
public class Tenancy
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public Guid TenantId { get; set; } // FK to Tenant
    public Guid PropertyListingId { get; set; } // FK to PropertyUnit
    public Guid? SubUnitId { get; set; } // FK to SubUnit
    public DateTime TenancyStart { get; set; }
    public DateTime? TenancyEnd { get; set; }
    public bool IsMonthlyRenewable { get; set; } = false; // default value enforced in DB level
    public bool IsTenancyActive { get; set; } = false; // default value enforced in DB level
    public DateTime? EvacuationDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}