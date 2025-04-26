namespace PropertyPilot.Api.Constants;

/// <summary>
/// Authentication Roles Policies
/// </summary>
public static class AuthPolicies
{
    /// <summary>
    /// only AdminManager Role
    /// </summary>
    public const string AdminManagerOnly = "AdminManagerOnly";

    /// <summary>
    /// only Caretaker Role
    /// </summary>
    public const string CaretakerOnly = "CaretakerOnly";

    /// <summary>
    /// Both AdminManager and Manager Roles
    /// </summary>
    public const string ManagerAndAbove = "ManagerAndAbove";

    /// <summary>
    /// All or AdminManager, Manager and Caretaker Roles
    /// </summary>
    public const string AllRoles = "AllRoles";
}
