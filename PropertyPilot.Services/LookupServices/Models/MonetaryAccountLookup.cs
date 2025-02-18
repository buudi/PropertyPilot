namespace PropertyPilot.Services.LookupServices.Models;

public record MonetaryAccountLookup
{
    public Guid Id { get; init; }
    public string AccountName { get; init; }
    public double Balance { get; init; }
}