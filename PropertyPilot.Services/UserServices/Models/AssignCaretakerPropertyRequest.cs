namespace PropertyPilot.Services.UserServices.Models;

public class AssignCaretakerPropertyRequest
{
    public required Guid UserId { get; set; }
    public required Guid PropertyId { get; set; }
}
