namespace PropertyPilot.Services.Constants;

public class Keys
{
    /// <summary>
    /// The GUID for the main monetary account.
    /// </summary>
    public static Guid MainMonetaryAccountGuid = Guid.Parse("7e174c5d-3756-4f9d-87b3-8f5e59f7f69e");

    /// <summary>
    /// The GUID for the Stripe monetary account.
    /// </summary>
    public static Guid StripeMonetaryAccountGuid = Guid.Parse("d24bde15-7ab2-46e9-9852-d99b51bc5e19");

    /// <summary>
    /// The GUID for the Stripe user account.
    /// </summary>
    public static Guid StripeUserAccountGuid = Guid.Parse("a83de33a-5bcc-43f1-8350-342159576e31");
}
