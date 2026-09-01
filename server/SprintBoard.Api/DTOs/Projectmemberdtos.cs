using SprintBoard.Api.Models;

namespace SprintBoard.Api.DTOs;

public record AddMemberDto(string Email);

public record ProjectMemberDto(
    int UserId,
    string Name,
    string? AvatarUrl,
    ProjectRole ProjectRole,
    DateTime JoinTime
);