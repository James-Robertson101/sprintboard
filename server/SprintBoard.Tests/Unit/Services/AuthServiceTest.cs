using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Moq;
using SprintBoard.Api.DTOs;
using SprintBoard.Api.Exceptions;
using SprintBoard.Api.Models;
using SprintBoard.Api.Repositories;
using SprintBoard.Api.Services;

namespace SprintBoard.Tests.Unit.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _usersMock;
    private readonly IConfiguration _config;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _usersMock = new Mock<IUserRepository>();
        _config = BuildConfig();
        _service = new AuthService(_usersMock.Object, _config);
    }

    private static IConfiguration BuildConfig()
    {
        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Secret"] = "this-is-a-test-signing-key-that-is-long-enough",
            ["Jwt:Issuer"] = "SprintBoard.Tests",
            ["Jwt:ExpiryHours"] = "1"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    // RegisterAsync
    [Fact]
    public async Task RegisterAsync_WithNewEmail_CreatesUserAndReturnsToken()
    {
        // Arrange
        var dto = new RegisterDto(
            "New User",
            "newuser@example.com",
            "Password123!",
            ""
        );

        _usersMock
            .Setup(r => r.FindByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);

        _usersMock
            .Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        // Act
        var (token, user) = await _service.RegisterAsync(dto);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.Equal(dto.Email, user.Email);
        Assert.Equal(dto.Name, user.Name);

        _usersMock.Verify(
            r => r.CreateAsync(It.Is<User>(u =>
                u.Email == dto.Email &&
                u.Name == dto.Name &&
                u.PasswordHash != null)),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ThrowsConflictException()
    {
        // Arrange
        var dto = new RegisterDto(
            "Existing User",
            "existing@example.com",
            "Password123!",
            ""
        );

        var existingUser = new User
        {
            Id = 1,
            Email = dto.Email,
            Name = "Existing User"
        };

        _usersMock
            .Setup(r => r.FindByEmailAsync(dto.Email))
            .ReturnsAsync(existingUser);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(
            () => _service.RegisterAsync(dto));

        _usersMock.Verify(
            r => r.CreateAsync(It.IsAny<User>()),
            Times.Never);
    }

    // LoginAsync
    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var password = "Password123!";

        var user = new User
        {
            Id = 1,
            Email = "user@example.com",
            Name = "Test User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        var dto = new LoginDto(user.Email, password);

        _usersMock
            .Setup(r => r.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        // Act
        var (token, resultUser) = await _service.LoginAsync(dto);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.Equal(user.Email, resultUser.Email);
    }

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var dto = new LoginDto("missing@example.com", "Password123!");

        _usersMock
            .Setup(r => r.FindByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync(dto));
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsIncorrect_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "user@example.com",
            Name = "Test User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword1!")
        };

        var dto = new LoginDto(user.Email, "WrongPassword1!");

        _usersMock
            .Setup(r => r.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync(dto));
    }

    [Fact]
    public async Task LoginAsync_WhenUserHasNoPasswordHash_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        // Simulates a Google-only account that has never set a password.
        var user = new User
        {
            Id = 1,
            Email = "googleuser@example.com",
            Name = "Google User",
            PasswordHash = null
        };

        var dto = new LoginDto(user.Email, "SomePassword1!");

        _usersMock
            .Setup(r => r.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync(dto));
    }

    // GenerateJwt
    [Fact]
    public void GenerateJwt_ReturnsTokenWithExpectedClaims()
    {
        // Arrange
        var user = new User
        {
            Id = 42,
            Email = "claims@example.com",
            Name = "Claims User",
            Role = UserRole.User
        };

        // Act
        var token = _service.GenerateJwt(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Equal(
            user.Id.ToString(),
            jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

        Assert.Equal(
            user.Email,
            jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value);

        Assert.Equal(
            user.Name,
            jwt.Claims.First(c => c.Type == ClaimTypes.Name).Value);

        Assert.Equal(
            user.Role.ToString(),
            jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);

        Assert.Equal("SprintBoard.Tests", jwt.Issuer);
    }

    // LoginWithGoogleAsync
    [Fact]
    public async Task LoginWithGoogleAsync_WhenGoogleIdAlreadyLinked_ReturnsExistingUser()
    {
        // Arrange
        var googleId = "google-123";
        var email = "linked@example.com";
        var name = "Linked User";

        var existingUser = new User
        {
            Id = 1,
            Email = email,
            Name = name,
            GoogleId = googleId
        };

        _usersMock
            .Setup(r => r.FindByGoogleIdAsync(googleId))
            .ReturnsAsync(existingUser);

        // Act
        var (token, user) = await _service.LoginWithGoogleAsync(
            googleId, email, name);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.Equal(email, user.Email);

        _usersMock.Verify(
            r => r.FindByEmailAsync(It.IsAny<string>()),
            Times.Never);

        _usersMock.Verify(
            r => r.CreateAsync(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginWithGoogleAsync_WhenEmailExistsButNotLinked_LinksGoogleAccount()
    {
        // Arrange
        var googleId = "google-456";
        var email = "existing@example.com";
        var name = "Existing User";

        var existingUser = new User
        {
            Id = 2,
            Email = email,
            Name = name,
            GoogleId = null
        };

        _usersMock
            .Setup(r => r.FindByGoogleIdAsync(googleId))
            .ReturnsAsync((User?)null);

        _usersMock
            .Setup(r => r.FindByEmailAsync(email))
            .ReturnsAsync(existingUser);

        _usersMock
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        var (token, user) = await _service.LoginWithGoogleAsync(
            googleId, email, name);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.Equal(email, user.Email);
        Assert.Equal(googleId, existingUser.GoogleId);

        _usersMock.Verify(
            r => r.UpdateAsync(It.Is<User>(u => u.GoogleId == googleId)),
            Times.Once);

        _usersMock.Verify(
            r => r.CreateAsync(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginWithGoogleAsync_WhenNoExistingUser_CreatesNewUser()
    {
        // Arrange
        var googleId = "google-789";
        var email = "brandnew@example.com";
        var name = "Brand New User";

        _usersMock
            .Setup(r => r.FindByGoogleIdAsync(googleId))
            .ReturnsAsync((User?)null);

        _usersMock
            .Setup(r => r.FindByEmailAsync(email))
            .ReturnsAsync((User?)null);

        _usersMock
            .Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        // Act
        var (token, user) = await _service.LoginWithGoogleAsync(
            googleId, email, name);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.Equal(email, user.Email);
        Assert.Equal(name, user.Name);

        _usersMock.Verify(
            r => r.CreateAsync(It.Is<User>(u =>
                u.Email == email &&
                u.Name == name &&
                u.GoogleId == googleId &&
                u.PasswordHash == null)),
            Times.Once);

        _usersMock.Verify(
            r => r.UpdateAsync(It.IsAny<User>()),
            Times.Never);
    }
}