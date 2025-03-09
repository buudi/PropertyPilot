namespace PropertyPilot.Services.PropertyListingServices.Models;

public class TenantsTimelineResponse
{
    public Guid TenantId { get; set; }
    public string TenantName { get; set; }
    public DateTime TenancyStart { get; set; }
    public DateTime? TenancyEnd { get; set; }
    public Guid SubUnitId { get; set; }
}

public class TimelineResponse
{
    public List<SubUnitTimelineResponse> SubUnitTimelineResponse { get; set; } = [];
    public List<TenantsTimelineResponse> TenantsTimelineResponse { get; set; } = [];
}