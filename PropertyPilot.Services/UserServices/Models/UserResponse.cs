namespace PropertyPilot.Services.UserServices.Models;

public record UserResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string Role { get; init; }
    public required bool HasAccess { get; init; }
    public required DateTime LastLogin { get; init; }
    public required DateTime CreatedOn { get; init; }
}
