namespace PropertyPilot.Services.PropertiesServices.Models;

public record PropertyListingRecord
{
    public required Guid Id { get; init; }

    public required string PropertyName { get; init; }

    public string? Emirate { get; init; }

    public required string PropertyType { get; init; }

    public required string Occupancy { get; init; }

    public required double Revenue { get; init; }

    public required double Expenses { get; init; }

    public required string Caretaker { get; init; }
}
