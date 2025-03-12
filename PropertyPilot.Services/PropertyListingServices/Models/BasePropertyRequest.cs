using System.ComponentModel.DataAnnotations;

namespace PropertyPilot.Services.PropertiesServices.Models;

public record BasePropertyRequest
{
    [Required]
    public string PropertyName { get; init; } = string.Empty;

    public string Emirate { get; init; } = string.Empty;

    [Required]
    public string PropertyType { get; init; } = string.Empty;

    public int UnitsCount { get; init; }
}
