namespace SprintBoard.Api.Services;
using SprintBoard.Api.DTOs;
public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}