using System.ComponentModel.DataAnnotations;

namespace PropertyPilot.Api.Services.PropertyListingServices.Models;

public class CreatePropertyListingRequest
{
    [Required]
    public string PropertyName { get; set; } = string.Empty;

    public string? Emirate { get; set; }

    [Required]
    public string PropertyType { get; set; } = string.Empty;

    public int? UnitsCount { get; set; }
}
