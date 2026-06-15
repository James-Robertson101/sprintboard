using SprintBoard.Api.DTOs;

public record AuthResponseDto(
  string Token,
  UserDto User
);