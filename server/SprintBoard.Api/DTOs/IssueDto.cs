using SprintBoard.Api.Models;

namespace SprintBoard.Api.DTOs;

public record IssueDto (
  string Name,
  string Description,
  Priority Priority,
  User? Asignee
);
