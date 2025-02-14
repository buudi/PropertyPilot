using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyPilot.Dal.Models;

[Table(nameof(Transaction))]
public class Transaction
{
    public static class TransactionTypes
    {
        public const string Transfer = nameof(Transfer);
        public const string RentPayment = nameof(RentPayment);
        public const string Expense = nameof(Expense);
        public const string ReturnedRent = nameof(ReturnedRent);
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public string TransactionType { get; set; } = TransactionTypes.Transfer;
    public Guid ReferenceId { get; set; } // ID to corresponding TransactionType table
    public Guid? SourceAccountId { get; set; } // account debited, fk to MonetaryAccount
    public Guid? DestinationAccountId { get; set; } // account credited, fk to MonetaryAccount
    public double Amount { get; set; } = 0.0;
    public string Currency { get; set; } = "AED";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}