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