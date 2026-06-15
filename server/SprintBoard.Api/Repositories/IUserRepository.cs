namespace SprintBoard.Api.Repositories;
using SprintBoard.Api.Models;
public interface IUserRepository
{
    Task<User> CreateAsync(User user);
    Task<User?> GetByIdAsync(int id);
    Task<User?> FindByEmailAsync(string email);
}