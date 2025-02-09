using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyPilot.Dal.Models;

[Table(nameof(InvoiceItem))]
public class InvoiceItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public double Amount { get; set; }
    public Guid InvoiceId { get; set; } // FK to Invoice
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}