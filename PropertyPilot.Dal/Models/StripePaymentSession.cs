using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyPilot.Dal.Models;

[Table(nameof(StripePaymentSession))]
public class StripePaymentSession
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public string StripeSessionId { get; set; } = null!;
    public Guid TenantId { get; set; }
    public string InvoiceIds { get; set; } = null!; // Comma-separated list`
    public double TotalAmount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Completed, Failed
    public string? StripePaymentIntentId { get; set; }
    public string? StripeReceiptUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}