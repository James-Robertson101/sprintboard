using SprintBoard.Api.DTOs;
public interface IUserService
{
  Task<UserDto> CreateFromEmailAsync(RegisterDto dto);
  Task<UserDto?> GetByIdAsync(int id); 
}