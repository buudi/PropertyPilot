namespace PropertyPilot.Dal.Abstractions;

public interface ITenant
{
    Guid Id { get; set; }

    string Name { get; set; }

    string PhoneNumber { get; set; }

    string Email { get; set; }

    string TenantIdentification { get; set; }

    bool IsAccountActive { get; set; }
}
