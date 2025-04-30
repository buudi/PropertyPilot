namespace PropertyPilot.Services.CaretakerPortalServices.Models.Finances;

public class TransactionHistoryViewRecord
{
    public string? TransactionType { get; set; }
    public string? ReferenceId { get; set; }

    // if rent payment or expense, reference text is the property name, if transfer then null
    public string? ReferenceText { get; set; }

    // if rent payment, PersonName is the tenant name, if expense or transfer then null
    public string? PersonName { get; set; }

    // if transfer, ToAccountName is the destination account name, if rent payment or expense then null
    public string? ToAccountName { get; set; }
    public double Amount { get; set; } // if transaction is to main account (deposit to main account) this amount would be negative
    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }
}
