namespace SprintBoard.Api.DTOs;

public record RegisterDto(
    string Name,
    string Email,
    string Password
);