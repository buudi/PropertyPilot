namespace PropertyPilot.Services.CaretakerPortalServices.Models;

public class AssignedApartment
{
    public required Guid Id { get; set; }
    public string PropertyName { get; set; } = string.Empty;
    public string PropertyAddress { get; set; } = string.Empty;
    public int VacanciesCount { get; set; }
    public int SubUnitsCount { get; set; }
    public double OutstandingBalance { get; set; }
    public int TenantsLeavingThisMonth { get; set; }
}