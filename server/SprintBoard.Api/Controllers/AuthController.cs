using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SprintBoard.Api.DTOs;
using SprintBoard.Api.Services;

namespace SprintBoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly IUserService _userService;
    private readonly IConfiguration _config;

    public AuthController(
        IAuthService auth,
        IUserService userService,
        IConfiguration config)
    {
        _auth = auth;
        _userService = userService;
        _config = config;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto dto)
    {
        try
        {
            var (token, user) = await _auth.RegisterAsync(dto);

            var expiry = DateTimeOffset.UtcNow.AddHours(
                double.Parse(_config["Jwt:ExpiryHours"]!)
            );

            Response.Cookies.Append(
                "access_token",
                token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = expiry,
                    Path = "/"
                });

            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto dto)
    {
        try
        {
            var (token, user) = await _auth.LoginAsync(dto);

            var expiry = DateTimeOffset.UtcNow.AddHours(
                double.Parse(_config["Jwt:ExpiryHours"]!)
            );

            Response.Cookies.Append(
                "access_token",
                token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = expiry,
                    Path = "/"
                });

            return Ok(user);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    [AllowAnonymous]
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

    [AllowAnonymous]
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

        var (token, user) = await _auth.LoginWithGoogleAsync(
            googleId,
            email,
            name);
        await HttpContext.SignOutAsync("GoogleTemporary");
        var expiry = DateTimeOffset.UtcNow.AddHours(
            double.Parse(_config["Jwt:ExpiryHours"]!)
        );

        Response.Cookies.Append(
            "access_token",
            token,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = expiry,
                Path = "/"
            });
            var frontendUrl = _config["FrontendUrl"];

        return Redirect($"{frontendUrl}/projectList");
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (userIdClaim is null ||
            !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _userService.GetByIdAsync(userId);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }
}