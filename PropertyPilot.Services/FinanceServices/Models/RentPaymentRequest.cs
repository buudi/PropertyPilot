namespace PropertyPilot.Services.FinanceServices.Models;

public class RentPaymentRequest
{
    public required Guid TenantId { get; set; }
    public required Guid InvoiceId { get; set; }
    public required double Amount { get; set; }
    public required string PaymentMethod { get; set; }
}