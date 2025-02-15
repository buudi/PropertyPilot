using PropertyPilot.Dal.Models;
using Transaction = PropertyPilot.Dal.Models.Transaction;

namespace PropertyPilot.Services.FinanceServices.Models;

public record RentPaymentTransactionRecord
{
    public required RentPayment RentPayment { get; init; }
    public required Transaction Transaction { get; init; }
}