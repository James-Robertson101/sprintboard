// Services/AuthService.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SprintBoard.Api.DTOs;
using SprintBoard.Api.Models;
using SprintBoard.Api.Repositories;

namespace SprintBoard.Api.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository users, IConfiguration config)
    {
        _users = users;
        _config = config;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // Check email not already taken
        var existing = await _users.FindByEmailAsync(dto.Email);
        if (existing is not null)
            throw new InvalidOperationException("Email already in use.");

        var user = new User
        {
            Email        = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Name     = dto.Name,
            CreatedAt    = DateTime.UtcNow
        };

        await _users.CreateAsync(user);
        var token = GenerateJwt(user);
        return new AuthResponseDto(token, MapToDto(user));
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _users.FindByEmailAsync(dto.Email);

        // Same error for "not found" and "wrong password" — don't leak which one
        if (user is null || user.PasswordHash is null ||
            !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var token = GenerateJwt(user);
        return new AuthResponseDto(token, MapToDto(user));
    }

    // Internal — also called by GoogleService later
    public string GenerateJwt(User user)
    {   
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("Name", user.Name),
        };

        var token = new JwtSecurityToken(
            issuer:             _config["Jwt:Issuer"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(
                                    double.Parse(_config["Jwt:ExpiryHours"]!)),
            signingCredentials: new SigningCredentials(
                                    key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<AuthResponseDto> LoginWithGoogleAsync(string googleId, string email,string name)
{
    var user = await _users.FindByGoogleIdAsync(googleId);

    if (user is null)
    {
        user = await _users.FindByEmailAsync(email);

        if (user is not null)
        {
            // Existing account with same email.
            // Link the Google account.
            user.GoogleId = googleId;
            user.UpdatedAt = DateTime.UtcNow;

            await _users.UpdateAsync(user);
        }
        else
        {
            // Completely new Google user.
            user = new User
            {
                Name = name,
                Email = email,
                GoogleId = googleId,
                PasswordHash = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _users.CreateAsync(user);
        }
    }

    var token = GenerateJwt(user);

    return new AuthResponseDto(token, MapToDto(user));
}

    private static UserDto MapToDto(User user) =>
        new(user.Id, user.Email, user.Name, user.AvatarUrl, user.Role);
}