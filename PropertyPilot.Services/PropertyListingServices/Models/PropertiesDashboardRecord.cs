namespace PropertyPilot.Services.PropertiesServices.Models;

public record PropertiesDashboardRecord
{
    public required int TotalProperties { get; init; }

    public required int VacantUnits { get; init; }

    public required double OccupancyRate { get; init; }

    public required double AverageMonthlyRevenue { get; init; }
}