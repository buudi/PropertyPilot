using PropertyPilot.Services.CaretakerPortalServices.Models.Finances;

namespace PropertyPilot.Services.CaretakerPortalServices.Models.Responses;

public class CaretakerPortalFinancesScreen
{
    public double CurrentBalance { get; set; }
    public double CollectedThisMonth { get; set; }
    public List<TransactionHistoryViewRecord> TransactionHistoryRecords { get; set; } = [];
}
