using SprintBoard.Api.DTOs;
public interface IUserService
{
  Task<UserDto?> GetByIdAsync(int id);
  Task<UserDto?> UpdateProfileAsync(int userId, UpdateProfileDto dto);
  Task<UserDeletionResult> DeleteAsync(int userId, int requestingUserId);
}