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

    public string? email { get; set; }

    // todo: foreign key to current active contract

    [Required]
    public required string LifecycleStatus { get; set; }

    [Required]
    public required DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public DateTime? DateArchived { get; set; }
}
