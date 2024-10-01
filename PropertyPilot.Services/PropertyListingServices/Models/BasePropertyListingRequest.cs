using System.ComponentModel.DataAnnotations;

namespace PropertyPilot.Services.PropertyListingServices.Models;

public record BasePropertyListingRequest
{
    [Required]
    public string PropertyName { get; init; } = string.Empty;

    public string? Emirate { get; init; }

    [Required]
    public string PropertyType { get; init; } = string.Empty;

    public int? UnitsCount { get; init; }
}
