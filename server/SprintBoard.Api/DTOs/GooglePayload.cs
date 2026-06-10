namespace SprintBoard.Api.DTOs;

public record GooglePayload(
    string Sub,      // this becomes GoogleId
    string Email,
    string Name,
    string? Picture  // this becomes AvatarUrl
);