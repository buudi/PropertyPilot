namespace PropertyPilot.Services.InvoiceServices.Models;

public class CreateInvoiceRequest
{
    public double? Discount { get; set; }
    public required Guid TenantId { get; set; }
    public required DateTime DateStart { get; set; }
    public DateTime? DateDue { get; set; }
    public required bool IsRenewable { get; set; }
    public required string InvoiceStatus { get; set; }
}
