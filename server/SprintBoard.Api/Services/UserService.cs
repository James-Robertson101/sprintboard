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

    public async Task<UserDto> CreateFromEmailAsync(RegisterDto dto)
    {
        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        var created = await _userRepository.CreateAsync(user);

        return new UserDto(created.Id, created.Name, created.Email, created.AvatarUrl, created.Role);
    }

    public async Task<UserDto?> GetByIdAsync(int id)
  {
    var user = await _userRepository.FindByIdAsync(id);
    if (user is null) return null;
    return new UserDto(user.Id, user.Name, user.Email, user.AvatarUrl, user.Role);
  }
}