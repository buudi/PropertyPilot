namespace PropertyPilot.Services.LookupServices.Models;

public record PropertyListingsLookup
{
    public required Guid Id { get; init; }
    public required string PropertyName { get; init; }
    public required string PropertyType { get; init; }
    public required int UnitsCount { get; init; }
}
