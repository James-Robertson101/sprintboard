namespace SprintBoard.Api.DTOs;

public record UpdateUserDto(
    string? Name,
    string? AvatarUrl
);
// GoogleId intentionally excluded