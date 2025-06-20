using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.JwtServices;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PropertyPilot.Api.Controllers.AuthsController;

[ApiController]
[Route("api/auth")]

public class AuthController(PmsDbContext pmsDbContext, JwtService jwtService) : ControllerBase
{
    [Authorize]
    [HttpGet("me")]
    public IActionResult GetMyAccount()
    {
        var authorizationHeader = Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
        {
            return Unauthorized(new { message = "Missing or invalid Authorization header" });
        }

        try
        {
            var token = authorizationHeader.Substring("Bearer ".Length).Trim();
            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(token))
            {
                return Unauthorized(new { message = "Invalid token format" });
            }

            var jwtToken = handler.ReadJwtToken(token);
            var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(emailClaim))
            {
                return Unauthorized(new { message = "Email claim not found in token" });
            }

            return Ok(new { email = emailClaim });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = "Token validation failed", error = ex.Message });
        }
    }

    [HttpPost("signups/admin-manager")]
    public IActionResult CreateAdminManagerAccount([FromBody] SignUpModel model)
    {
        if (pmsDbContext.PropertyPilotUsers.Any(u => u.Email == model.Email))
        {
            return BadRequest("User with this email already exists");
        }

        var user = new PropertyPilotUser
        {
            Name = model.Name,
            Email = model.Email,
            HashedPassword = HashPassword(model.Password),
            Role = PropertyPilotUser.UserRoles.AdminManager
        };

        pmsDbContext.PropertyPilotUsers.Add(user);
        pmsDbContext.SaveChanges();

        return Ok("Admin Manager account created successfully");
    }

    [HttpPost("signups/manager")]
    public IActionResult CreateManagerAccount([FromBody] SignUpModel model)
    {
        if (pmsDbContext.PropertyPilotUsers.Any(u => u.Email == model.Email))
        {
            return BadRequest("User with this email already exists");
        }

        var user = new PropertyPilotUser
        {
            Name = model.Name,
            Email = model.Email,
            HashedPassword = HashPassword(model.Password),
            Role = PropertyPilotUser.UserRoles.Manager
        };

        pmsDbContext.PropertyPilotUsers.Add(user);
        pmsDbContext.SaveChanges();

        return Ok("Admin Manager account created successfully");
    }

    [HttpPost("signups/caretaker")]
    public IActionResult CreateCaretakerAccount([FromBody] SignUpModel model)
    {
        if (pmsDbContext.PropertyPilotUsers.Any(u => u.Email == model.Email))
        {
            return BadRequest("User with this email already exists");
        }

        var user = new PropertyPilotUser
        {
            Name = model.Name,
            Email = model.Email,
            HashedPassword = HashPassword(model.Password),
            Role = PropertyPilotUser.UserRoles.Caretaker
        };

        pmsDbContext.PropertyPilotUsers.Add(user);
        pmsDbContext.SaveChanges();

        return Ok("Caretaker account created successfully");
    }

    /// <summary>
    /// tenant sign up
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("signups/tenant")]
    public IActionResult CreateTenantAccount([FromBody] TenantSignUpModel model)
    {
        if (pmsDbContext.TenantAccounts.Any(a => a.Email == model.Email))
        {
            return BadRequest("Tenant account with this email already exists");
        }

        using var transaction = pmsDbContext.Database.BeginTransaction();
        try
        {
            var tenant = pmsDbContext.Tenants.FirstOrDefault(t => t.Email == model.Email);
            if (tenant != null)
            {
                return BadRequest("Tenant with this email already exists");
            }

            tenant = new Tenant
            {
                Name = model.Name,
                TenantIdentification = model.EmiratesId,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email
            };
            pmsDbContext.Tenants.Add(tenant);
            pmsDbContext.SaveChanges();

            var tenantAccount = new TenantAccount
            {
                Email = model.Email,
                HashedPassword = HashPassword(model.Password),
                CreatedOn = DateTime.UtcNow,
                IsArchived = false,
                HasAccess = true,
                LastLogin = DateTime.UtcNow,
                TenantId = tenant.Id
            };

            pmsDbContext.TenantAccounts.Add(tenantAccount);
            pmsDbContext.SaveChanges();

            transaction.Commit();
            return Created();
        }
        catch
        {
            throw;
        }
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel model)
    {
        var user = pmsDbContext.PropertyPilotUsers.FirstOrDefault(u => u.Email == model.Email);

        if (user == null)
        {
            return Unauthorized("User not found.");
        }

        if (!VerifyPassword(model.Password, user.HashedPassword))
        {
            return Unauthorized("Incorrect password.");
        }

        if (user.IsArchived)
        {
            return Unauthorized("User account is archived.");
        }


        var accessToken = jwtService.GenerateAccessToken(user);
        var refreshToken = jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        user.LastLogin = DateTime.UtcNow;
        pmsDbContext.SaveChanges();

        return Ok(new
        {
            User = user,
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });
    }

    /// <summary>
    /// tenants login
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("tenants/login")]
    public IActionResult TenantLogin([FromBody] LoginModel model)
    {
        var tenantAccount = pmsDbContext.TenantAccounts.FirstOrDefault(a => a.Email == model.Email);

        if (tenantAccount == null)
        {
            return Unauthorized("Tenant account not found.");
        }

        if (!VerifyPassword(model.Password, tenantAccount.HashedPassword))
        {
            return Unauthorized("Incorrect password.");
        }

        if (tenantAccount.IsArchived)
        {
            return Unauthorized("Tenant account is archived.");
        }

        var accessToken = jwtService.GenerateAccessToken(tenantAccount);
        var refreshToken = jwtService.GenerateRefreshToken();

        tenantAccount.RefreshToken = refreshToken;
        tenantAccount.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        tenantAccount.LastLogin = DateTime.UtcNow;
        pmsDbContext.SaveChanges();

        return Ok(new
        {
            TenantAccount = tenantAccount,
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });
    }

    [HttpPost("refresh-token")]
    public IActionResult RefreshToken([FromBody] RefreshTokenModel model)
    {
        if (model is null)
        {
            return BadRequest("Invalid client request");
        }

        string accessToken = model.AccessToken;
        string refreshToken = model.RefreshToken;

        var principal = jwtService.GetPrincipalFromExpiredToken(accessToken);
        var email = principal.FindFirstValue(ClaimTypes.Email);

        var user = pmsDbContext.PropertyPilotUsers.SingleOrDefault(u => u.Email == email);

        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
        {
            return BadRequest("Invalid client request");
        }

        var newAccessToken = jwtService.GenerateAccessToken(user);
        var newRefreshToken = jwtService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        pmsDbContext.SaveChanges();

        return Ok(new { AccessToken = newAccessToken, RefreshToken = newRefreshToken });
    }

    [HttpPost("tenants/refresh-token")]
    public IActionResult TenantRefreshToken([FromBody] RefreshTokenModel model)
    {
        if (model is null)
        {
            return BadRequest("Invalid client request");
        }

        string accessToken = model.AccessToken;
        string refreshToken = model.RefreshToken;

        var principal = jwtService.GetPrincipalFromExpiredToken(accessToken);
        var email = principal.FindFirstValue(ClaimTypes.Email);

        var tenantAccount = pmsDbContext.TenantAccounts.SingleOrDefault(a => a.Email == email);

        if (tenantAccount == null || tenantAccount.RefreshToken != refreshToken || tenantAccount.RefreshTokenExpiryTime <= DateTime.Now)
        {
            return BadRequest("Invalid client request");
        }

        var newAccessToken = jwtService.GenerateAccessToken(tenantAccount);
        var newRefreshToken = jwtService.GenerateRefreshToken();

        tenantAccount.RefreshToken = newRefreshToken;
        tenantAccount.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        pmsDbContext.SaveChanges();

        return Ok(new { AccessToken = newAccessToken, RefreshToken = newRefreshToken });
    }

    [Authorize]
    [HttpPost("revoke-token")]
    public IActionResult RevokeToken()
    {
        if (User.Identity is null)
        {
            return Unauthorized();
        }

        var email = User.Identity.Name;
        var user = pmsDbContext.PropertyPilotUsers.SingleOrDefault(u => u.Email == email);
        if (user == null) return BadRequest();

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        pmsDbContext.SaveChanges();

        return NoContent();
    }

    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }
    }

    private bool VerifyPassword(string password, string hashedPassword)
    {
        return HashPassword(password) == hashedPassword;
    }
}

public class SignUpModel
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}

public class TenantSignUpModel
{
    public string Email { get; set; }
    public string Name { get; set; }
    public string EmiratesId { get; set; }
    public string PhoneNumber { get; set; }
    public string Password { get; set; }
}


public class LoginModel
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class RefreshTokenModel
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

