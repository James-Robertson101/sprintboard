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

public async Task<List<User>> SearchUsersAsync(string? search)
{
    var query = _db.Users.AsQueryable();

    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(u =>
            u.Name.Contains(search) || u.Email.Contains(search));
    }

    return await query.ToListAsync();
}

 public async Task<bool> IsSoleOwnerOfAnyProjectAsync(int userId)
    {
        return await _db.ProjectMembers
            .Where(pm =>
                pm.RemovedTime == null &&
                pm.ProjectRole == ProjectRole.Owner)
            .GroupBy(pm => pm.ProjectId)
            .AnyAsync(g =>
                g.Count() == 1 &&
                g.Any(pm => pm.UserId == userId));
    }

public async Task RemoveActiveProjectMembershipsAsync(int userId)
    {
        var memberships = await _db.ProjectMembers
            .Where(pm =>
                pm.UserId == userId &&
                pm.RemovedTime == null)
            .ToListAsync();

        var now = DateTime.UtcNow;

        foreach (var membership in memberships)
        {
            membership.RemovedTime = now;
        }
    }
}