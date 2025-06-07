namespace PropertyPilot.Services.MigrationServices.Models;

public class MigrationRecord
{
    public required string RoomIdentifier { get; set; }
    public string? TenantName { get; set; }
    public DateTime? Date { get; set; }
    public decimal? AssignedRent { get; set; }
    public decimal? Bank { get; set; }
    public decimal? Cash { get; set; }
    public decimal? Remaining { get; set; }
}
