using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Core.Application.Commands;
using Core.Infrastructure.Identity;
using MediatR;

namespace Core.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(IMediator mediator, IConfiguration configuration, ILogger<AuthController> logger, UserManager<ApplicationUser> userManager)
    {
        _mediator = mediator;
        _configuration = configuration;
        _logger = logger;
        _userManager = userManager;
    }

    [HttpGet("google")]
    public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action("GoogleCallback")
        };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        
        if (!result.Succeeded)
        {
            _logger.LogWarning("Google authentication failed");
            return BadRequest("Google authentication failed");
        }

        var claims = result.Principal!.Claims;
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var firstName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
        var lastName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;
        var googleId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var profilePicture = claims.FirstOrDefault(c => c.Type == "picture")?.Value;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
        {
            _logger.LogWarning("Missing required claims from Google authentication");
            return BadRequest("Missing required user information");
        }

        try
        {
            // Register or get existing user
            var user = await _mediator.Send(new RegisterUserCommand
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                GoogleId = googleId,
                ProfilePictureUrl = profilePicture
            });

            // Generate JWT token
            var token = GenerateJwtToken(user.Id, email, firstName, lastName);

            // Redirect to frontend with token and user data
            var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
            var redirectUrl = $"{frontendUrl}/auth-callback?token={token}&user={Uri.EscapeDataString(System.Text.Json.JsonSerializer.Serialize(user))}";
            
            return Redirect(redirectUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Google authentication");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("refresh")]
    [Authorize]
    public IActionResult RefreshToken()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var firstName = User.FindFirst(ClaimTypes.GivenName)?.Value;
        var lastName = User.FindFirst(ClaimTypes.Surname)?.Value;

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
        {
            return Unauthorized();
        }

        var token = GenerateJwtToken(Guid.Parse(userId), email, firstName, lastName);
        return Ok(new { Token = token });
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        // In a real application, you might want to blacklist the token
        return Ok(new { Message = "Logged out successfully" });
    }

    private string GenerateJwtToken(Guid userId, string email, string? firstName, string? lastName)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.GivenName, firstName ?? ""),
            new Claim(ClaimTypes.Surname, lastName ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
