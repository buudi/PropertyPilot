using PropertyPilot.Dal.Models;

namespace PropertyPilot.Services.FinanceServices.Models;

public record RentPaymentTransactionRecord
{
    public required RentPayment RentPayment { get; init; }
    public required Transaction Transaction { get; init; }
}