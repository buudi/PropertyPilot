namespace PropertyPilot.Services.CaretakerPortalServices.Models.Responses;

public class CaretakerPortalHomeScreen
{
    public string CaretakerFirstName { get; set; } = string.Empty;
    public double AccountBalance { get; set; }
    public List<AssignedApartment> AssignedApartmentsList { get; set; } = [];
}
