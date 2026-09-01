using SprintBoard.Api.Models;

namespace SprintBoard.Api.DTOs;

public record CreateIssueDto(
    string Name,
    string? Description,
    Priority Priority,
    int? AssigneeId
);

public record UpdateIssueDto(
    string Name,
    string? Description,
    Priority Priority,
    IssueStatus Status,
    int? AssigneeId
);

public record AssigneeDto(
    int Id,
    string Name,
    string? AvatarUrl
);

public record IssueResponseDto(
    int Id,
    string Name,
    string? Description,
    Priority Priority,
    IssueStatus Status,
    AssigneeDto? Assignee,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);