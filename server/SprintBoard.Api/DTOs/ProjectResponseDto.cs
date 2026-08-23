namespace SprintBoard.Api.DTOs;

public record ProjectResponseDto(
    int Id,
    string Name,
    string? Description,
    string? Icon
);