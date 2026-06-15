using SprintBoard.Api.DTOs;
using SprintBoard.Api.Models;
using SprintBoard.Api.Repositories;

namespace SprintBoard.Api.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> GetByIdAsync(int id)
  {
    var user = await _userRepository.GetByIdAsync(id);
    if (user is null) return null;
    return new UserDto(user.Id, user.Name, user.Email, user.AvatarUrl, user.Role);
  }
}