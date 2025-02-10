using PropertyPilot.Dal.Models;

namespace PropertyPilot.Services.InvoiceServices.Models;

public record InvoiceRecord
{
    public required Invoice Invoice { get; init; }
    public required List<InvoiceItem> InvoiceItems { get; init; } = [];
}
