using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.JwtServices;
using System.Security.Cryptography;
using System.Text;

namespace PropertyPilot.Api.Controllers.AuthsController;

[ApiController]
[Route("/api/[Controller]")]

public class AuthController(PmsDbContext pmsDbContext, JwtService jwtService) : ControllerBase
{
    [HttpPost("signup")]
    public IActionResult SignUp([FromBody] SignUpModel model)
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
            Role = PropertyPilotUser.UserRoles.Caretaker // Default role, change as needed
        };

        pmsDbContext.PropertyPilotUsers.Add(user);
        pmsDbContext.SaveChanges();

        return Ok("User created successfully");
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel model)
    {
        var user = pmsDbContext.PropertyPilotUsers.FirstOrDefault(u => u.Email == model.Email);

        if (user == null || !VerifyPassword(model.Password, user.HashedPassword))
        {
            return Unauthorized("Invalid email or password");
        }

        var accessToken = jwtService.GenerateAccessToken(user);
        var refreshToken = jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
        pmsDbContext.SaveChanges();

        return Ok(new { AccessToken = accessToken, RefreshToken = refreshToken });
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
        var username = principal.Identity.Name;

        var user = pmsDbContext.PropertyPilotUsers.SingleOrDefault(u => u.Email == username);

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

    [Authorize]
    [HttpPost("revoke-token")]
    public IActionResult RevokeToken()
    {
        var username = User.Identity.Name;
        var user = pmsDbContext.PropertyPilotUsers.SingleOrDefault(u => u.Email == username);
        if (user == null) return BadRequest();

        user.RefreshToken = null;
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

