using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyPilot.Dal.Models;

// a monetary account is created when a new PropertyPilotUser Is Created
[Table((nameof(MonetaryAccount)))]
public class MonetaryAccount
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public string AccountName { get; set; } = string.Empty;
    public Guid UserId { get; set; } = Guid.Empty; // if no user then empty guid

    public double Balance { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsClosed { get; set; } = false;
    public DateTime? DateClosed { get; set; }
}