using PropertyPilot.Services.CaretakerPortalServices.Models.Finances;
using PropertyPilot.Services.Generics;

namespace PropertyPilot.Services.CaretakerPortalServices.Models.Responses;

public class CaretakerPortalFinancesScreen
{
    public double CurrentBalance { get; set; }
    public double CollectedThisMonth { get; set; }
    public required PaginatedResult<TransactionHistoryViewRecord> TransactionHistoryRecords { get; set; }
}
