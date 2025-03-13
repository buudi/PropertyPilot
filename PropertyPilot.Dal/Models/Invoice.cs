using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyPilot.Dal.Models;

[Table(nameof(Invoice))]
public class Invoice
{
    public static class InvoiceStatuses
    {
        public const string Draft = "Draft";
        public const string Pending = "Pending";
        public const string Paid = "Paid";
        public const string Outstanding = "Outstanding";
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public Guid TenancyId { get; set; } // FK to Tenancy
    public double? Discount { get; set; }
    public Guid TenantId { get; set; } // FK to Tenant
    public DateTime DateStart { get; set; }
    public DateTime? DateDue { get; set; }
    public string InvoiceStatus { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
