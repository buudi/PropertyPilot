using System.ComponentModel.DataAnnotations;

namespace PropertyPilot.Services.Attributes.Validation;

public class GuidOrEmptyAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        if (value is string str && string.IsNullOrWhiteSpace(str))
        {
            return ValidationResult.Success;
        }

        if (value is Guid || Guid.TryParse(value.ToString(), out _))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult("Invalid GUID format.");
    }
}

