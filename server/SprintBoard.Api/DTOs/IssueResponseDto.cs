using SprintBoard.Api.Models;

namespace SprintBoard.Api.DTOs;

public record IssueResponseDto (
  int IssueId,
  string Name,
  string Description,
  Priority Priority,
  User Asignee
);
