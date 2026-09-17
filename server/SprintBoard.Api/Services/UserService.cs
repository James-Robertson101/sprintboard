using SprintBoard.Api.DTOs;
using SprintBoard.Api.Models;
using SprintBoard.Api.Repositories;

namespace SprintBoard.Api.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserDeletionService _deletionService;


    public UserService(IUserRepository userRepository, IUserDeletionService deletionService)
    {
        _userRepository = userRepository;
        _deletionService = deletionService;
    }

    public async Task<UserDto?> GetByIdAsync(int id)
  {
    var user = await _userRepository.GetByIdAsync(id);
    if (user is null) return null;
    return new UserDto(user.Id, user.Name, user.Email, user.AvatarUrl, user.Role);
  }

   public async Task<UserDto?> UpdateProfileAsync(
        int userId,
        UpdateProfileDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
            return null;

        user.Name = dto.Name.Trim();
        user.AvatarUrl = dto.AvatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        return new UserDto(
            user.Id,
            user.Name,
            user.Email,
            user.AvatarUrl,
            user.Role);
    }

    public Task<UserDeletionResult> DeleteAsync(int userId, int requestingUserId)
        => _deletionService.DeleteUserAsync(userId, requestingUserId);  

    public Task<UserDeletionResult> DeleteMyAccountAsync(int requestingUserId)
        => _deletionService.DeleteMyAccountAsync(requestingUserId);    

}