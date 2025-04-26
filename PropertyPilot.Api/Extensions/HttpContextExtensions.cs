using System.Security.Claims;

namespace PropertyPilot.Api.Extensions;

/// <summary>
/// Extensions for HttpContext.
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Get the user Guid from the HttpContext.
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static Guid GetUserId(this HttpContext ctx)
    {
        if (ctx?.User == null)
            throw new ArgumentNullException(nameof(ctx), "HttpContext or User is null.");

        //extract the user Guid from the ClaimsPrincipal    
        var userIdClaim = ctx.User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
            throw new InvalidOperationException("User ID claim not found.");

        return Guid.Parse(userIdClaim.Value);
    }
}
