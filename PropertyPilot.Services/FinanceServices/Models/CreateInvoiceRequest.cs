namespace PropertyPilot.Services.FinanceServices.Models;

public class CreateInvoiceRequest
{
    public Guid TenantId { get; set; }
    public Guid TenancyId { get; set; }
    public double? Discount { get; set; }
    public DateTime? DateDue { get; set; }
    public string? Notes { get; set; }
    public bool IsRenewable { get; set; }
    public List<CreateInvoiceItemRequest> InvoiceItems { get; set; } = [];
}


public class CreateInvoiceItemRequest
{
    public string Description { get; set; } = string.Empty;
    public double Amount { get; set; }
}