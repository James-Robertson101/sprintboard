// DTOs/ReportDtos.cs
using SprintBoard.Api.Models;

namespace SprintBoard.Api.DTOs;

public record StatusCountDto(IssueStatus Status, int Count);
public record PriorityCountDto(Priority Priority, int Count);

public record AssigneeWorkloadDto(
    int? UserId,
    string Name,          // "Unassigned" when null
    string? AvatarUrl,
    int Total,
    int Done);

public record SprintSummaryDto(
    int SprintId,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    SprintStatus Status,
    int TotalIssues,
    int CompletedIssues,
    double CompletionPercent,
    List<StatusCountDto> ByStatus,
    List<PriorityCountDto> ByPriority,
    List<AssigneeWorkloadDto> ByAssignee);

public record BurndownPointDto(DateTime Date, int Remaining, double Ideal);

public record VelocityPointDto(
    int SprintId,
    string SprintName,
    int Committed,
    int Completed);