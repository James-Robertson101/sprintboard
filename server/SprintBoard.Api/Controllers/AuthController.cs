// Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using SprintBoard.Api.DTOs;
using SprintBoard.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using System.Security.Claims;
namespace SprintBoard.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        try
        {
            var result = await _auth.RegisterAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        try
        {
            var (token,user) = await _auth.LoginAsync(dto);
            Response.Cookies.Append(
                "access_token",
                token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(5),
                    Path = "/"
                });
                
                var userDto = new UserDto(
                user.Id,
                user.Name,
                user.Email,
                user.AvatarUrl,
                user.Role
                );
            return Ok(userDto);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    [HttpGet("google")]
public IActionResult GoogleLogin()
{
    var properties = new AuthenticationProperties
    {
        RedirectUri = Url.Action(
            nameof(GoogleComplete),
            "Auth")
    };

    return Challenge(
        properties,
        GoogleDefaults.AuthenticationScheme);
}


[HttpGet("google/complete")]
public async Task<IActionResult> GoogleComplete()
{
    var result = await HttpContext.AuthenticateAsync(
        "GoogleTemporary");

    if (!result.Succeeded || result.Principal is null)
    {
        return Unauthorized();
    }

    var claims = result.Principal.Claims;

    var googleId = claims
        .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
        ?.Value;

    var email = claims
        .FirstOrDefault(c => c.Type == ClaimTypes.Email)
        ?.Value;

    var name = claims
        .FirstOrDefault(c => c.Type == ClaimTypes.Name)
        ?.Value;

    if (googleId is null || email is null || name is null)
    {
        return Unauthorized(
            "Google account information was incomplete.");
    }

    var authResponse = await _auth.LoginWithGoogleAsync(
        googleId,
        email,
        name);

    Response.Cookies.Append(
        "access_token",
        authResponse.Token,
        new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(5),
            Path = "/"
        });

    return Ok(authResponse.User);
}
}