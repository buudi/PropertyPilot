using PropertyPilot.Dal.Models;

namespace PropertyPilot.Services.FinanceServices.Models;

public class TransactionRecord : Transaction
{
    public string? SourceAccountName { get; set; }
    public string? DestinationAccountName { get; set; }


    /// <summary>
    /// Populates the current object's properties with values from the given transaction.
    /// </summary>
    /// <param name="transaction">The source transaction to copy values from.</param>
    public void PopulateTransactionObject(Transaction transaction)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));

        Id = transaction.Id;
        TransactionType = transaction.TransactionType;
        ReferenceId = transaction.ReferenceId;
        SourceAccountId = transaction.SourceAccountId;
        DestinationAccountId = transaction.DestinationAccountId;
        Amount = transaction.Amount;
        Currency = transaction.Currency;
        CreatedAt = transaction.CreatedAt;
    }
}