namespace SprintBoard.Api.DTOs;

public record GoogleLoginDto(
    string GoogleToken  // raw token from frontend, service verifies it
);