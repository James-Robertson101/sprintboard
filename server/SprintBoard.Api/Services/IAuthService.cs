namespace SprintBoard.Api.Services;
using SprintBoard.Api.DTOs;
public interface IAuthService
{
Task<(string Token, UserDto User)> RegisterAsync(
    RegisterDto dto);

Task<(string Token, UserDto User)> LoginAsync(
    LoginDto dto);

Task<(string Token, UserDto User)> LoginWithGoogleAsync(
    string googleId,
    string email,
    string name);
}