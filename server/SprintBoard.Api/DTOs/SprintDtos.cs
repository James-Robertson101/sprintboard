using SprintBoard.Api.Models;

namespace SprintBoard.Api.DTOs;

public record CreateSprintDto(
    string Name,
    string? Goal,
    DateTime StartDate,
    DateTime EndDate
);

public record UpdateSprintDto(
    string Name,
    string? Goal,
    DateTime StartDate,
    DateTime EndDate
);

public record SprintResponseDto(
    int Id,
    int ProjectId,
    string Name,
    string? Goal,
    DateTime StartDate,
    DateTime EndDate,
    SprintStatus Status,
    int IssueCount
);