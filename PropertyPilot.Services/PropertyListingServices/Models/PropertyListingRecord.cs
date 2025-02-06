namespace PropertyPilot.Services.PropertiesServices.Models;

public record PropertyListingRecord
{
    public required Guid Id { get; init; }

    public required string PropertyName { get; init; }

    public required string Emirate { get; init; }

    public required string PropertyType { get; init; }

    public required string Occupancy { get; init; }

    public int UnitsCount { get; init; }

    public Guid? CaretakerId { get; init; }
    public string? CaretakerName { get; init; }
    public string? CaretakerEmail { get; init; }
}
