namespace PropertyPilot.Services.InvoiceServices.Models;

public class InvoiceListingItem
{
    public Guid Id { get; set; }
    public string TenantName { get; set; }
    public string PropertyUnitName { get; set; }
    public string? SubUnit { get; set; }
    public string InvoiceStatus { get; set; }
    public double Amount { get; set; }
    public DateTime IssuedDate { get; set; }
    public DateTime? DueDate { get; set; }
}
