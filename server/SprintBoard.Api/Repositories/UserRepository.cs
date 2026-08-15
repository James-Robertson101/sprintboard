using Microsoft.EntityFrameworkCore;
using SprintBoard.Api.Data;
using SprintBoard.Api.Models;

namespace SprintBoard.Api.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User> CreateAsync(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<User?> GetByIdAsync(int id)
{
    return await _db.Users.FindAsync(id);
}
    public async Task<User?> FindByEmailAsync(string email)
    {
        return await _db.Users.FirstOrDefaultAsync(u=> u.Email == email);
    }

    public async Task<User?> FindByGoogleIdAsync(string googleId)
{
    return await _db.Users
        .FirstOrDefaultAsync(u => u.GoogleId == googleId);
}

public async Task UpdateAsync(User user)
{
    _db.Users.Update(user);
    await _db.SaveChangesAsync();
}
}