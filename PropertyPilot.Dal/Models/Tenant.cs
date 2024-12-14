using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyPilot.Dal.Models;

[Table("tenants")]
public class Tenant
{
    public static class LifecycleStatuses
    {
        public const string Active = nameof(Active);
        public const string PendingRenewal = nameof(PendingRenewal);
        public const string MovingOut = nameof(MovingOut);
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    public required string Name { get; set; }

    public string? EmiratesId { get; set; }

    [Required]
    public required string PhoneNumber { get; set; }

    public string? Email { get; set; }

    /// <summary>
    /// current active contract for the tenant
    /// </summary>
    public Guid? CurrentContractId { get; set; } // Foreign key for CurrentContract

    [ForeignKey(nameof(CurrentContractId))]
    public Contract? CurrentContract { get; set; }

    [Required]
    public required string LifecycleStatus { get; set; }

    [Required]
    public required DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public DateTime? DateArchived { get; set; }
}
