using Moq;
using SprintBoard.Api.DTOs;
using SprintBoard.Api.Models;
using SprintBoard.Api.Repositories;
using SprintBoard.Api.Services;
using Xunit;

namespace SprintBoard.Api.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly UserService _sut; // system under test

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _sut = new UserService(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ReturnsMappedDto()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Name = "Ada Lovelace",
            Email = "ada@sprintboard.dev",
            AvatarUrl = "https://example.com/avatar.png",
            Role = UserRole.Admin,
            // Fields below should never leak into the DTO
            GoogleId = "google-123",
            PasswordHash = "super-secret-hash"
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result!.Id);
        Assert.Equal(user.Name, result.Name);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.AvatarUrl, result.AvatarUrl);
        Assert.Equal(user.Role, result.Role);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_DoesNotExposeSensitiveFields()
    {
        // Arrange
        var user = new User
        {
            Id = 2,
            Name = "Grace Hopper",
            Email = "grace@sprintboard.dev",
            AvatarUrl = null,
            Role = UserRole.User,
            GoogleId = "google-456",
            PasswordHash = "another-secret-hash"
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.GetByIdAsync(2);

        // Assert
        Assert.NotNull(result);
        // UserDto has no GoogleId/PasswordHash properties, so this is really a
        // compile-time guarantee — this test documents that intent and will
        // fail to compile if someone adds them back to the record.
        var dtoProperties = typeof(UserDto).GetProperties().Select(p => p.Name);
        Assert.DoesNotContain("GoogleId", dtoProperties);
        Assert.DoesNotContain("PasswordHash", dtoProperties);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserHasNullAvatarUrl_ReturnsDtoWithNullAvatarUrl()
    {
        // Arrange
        var user = new User
        {
            Id = 3,
            Name = "Linus Torvalds",
            Email = "linus@sprintboard.dev",
            AvatarUrl = null,
            Role = UserRole.User
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(3))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.GetByIdAsync(3);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result!.AvatarUrl);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_CallsRepositoryExactlyOnceWithGivenId()
    {
        // Arrange
        const int userId = 42;
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        await _sut.GetByIdAsync(userId);

        // Assert
        _userRepositoryMock.Verify(r => r.GetByIdAsync(userId), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.User)]
    public async Task GetByIdAsync_PreservesRoleAcrossAllRoleValues(UserRole role)
    {
        // Arrange
        var user = new User
        {
            Id = 5,
            Name = "Test User",
            Email = "test@sprintboard.dev",
            AvatarUrl = null,
            Role = role
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.GetByIdAsync(5);

        // Assert
        Assert.Equal(role, result!.Role);
    }
}