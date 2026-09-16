using SprintBoard.Api.Models;

namespace  SprintBoard.Api.DTOs;

public record UserDto(
    int Id,
    string Name,
    string Email,
    string? AvatarUrl,
    UserRole Role
);
// No GoogleId, no PasswordHash — never exposed

public record UserSummaryDto(int Id, string Name, string? AvatarUrl);

public record UpdateProfileDto(
    string Name,
    string? AvatarUrl
);

public record UserDeletionResult(bool Succeeded, string? Error, bool IsNotFound)
{
    public static UserDeletionResult Success() => new(true, null, false);
    public static UserDeletionResult Conflict(string error) => new(false, error, false);
    public static UserDeletionResult NotFound() => new(false, null, true);
}