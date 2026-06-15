using SprintBoard.Api.DTOs;
public interface IUserService
{
  Task<UserDto?> GetByIdAsync(int id);
}