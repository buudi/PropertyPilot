namespace PropertyPilot.Services.LookupServices.Models;

public class InvoiceLookup
{
    public Guid Id { get; set; }
    public DateTime DateStart { get; set; }

    public string InvoiceStatus { get; set; }
    public double AmountRemaining { get; set; }
}