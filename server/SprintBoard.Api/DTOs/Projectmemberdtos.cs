using SprintBoard.Api.Models;

namespace SprintBoard.Api.DTOs;

public record AddMemberDto(int UserId);

public record ProjectMemberDto(
    int UserId,
    string Name,
    string? AvatarUrl,
    ProjectRole ProjectRole,
    DateTime JoinTime
);