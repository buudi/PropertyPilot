using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyPilot.Dal.Models;

[Table(nameof(RentPayment))]
public class RentPayment
{
    public static class PaymentMethods
    {
        public const string Cash = nameof(Cash);
        public const string BankTransferToMain = nameof(BankTransferToMain);
        public const string StripePayment = nameof(StripePayment);
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public Guid? InvoiceId { get; set; } // fk to Invoice
    public Guid TenantId { get; set; } // fk Tenant tenant Id
    public double Amount { get; set; }
    public Guid ReceiverAccountId { get; set; } // fk to MonetaryAccount
    public required string PaymentMethod { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}