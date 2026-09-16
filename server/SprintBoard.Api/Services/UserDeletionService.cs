using Microsoft.EntityFrameworkCore;
using SprintBoard.Api.Data;
using SprintBoard.Api.Models;
using SprintBoard.Api.DTOs;

namespace SprintBoard.Api.Services;
public class UserDeletionService : IUserDeletionService
{
    private readonly AppDbContext _db;

    public UserDeletionService(AppDbContext db) => _db = db;

    public async Task<UserDeletionResult> DeleteUserAsync(int userId, int requestingUserId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            return UserDeletionResult.NotFound();

        if (user.Id == requestingUserId)
            return UserDeletionResult.Conflict("You can't delete your own account.");

        // Block if this user is the sole Owner on any project
        var soleOwnerProjectIds = await _db.ProjectMembers
            .Where(pm => pm.RemovedTime == null && pm.ProjectRole == ProjectRole.Owner)
            .GroupBy(pm => pm.ProjectId)
            .Where(g => g.Count() == 1 && g.Any(pm => pm.UserId == userId))
            .Select(g => g.Key)
            .ToListAsync();

        if (soleOwnerProjectIds.Count > 0)
        {
            return UserDeletionResult.Conflict(
                $"User is the sole owner of {soleOwnerProjectIds.Count} project(s). " +
                "Transfer ownership before deleting.");
        }

        using var tx = await _db.Database.BeginTransactionAsync();

        // Soft-remove active project memberships (mirrors your existing pattern)
        var activeMemberships = await _db.ProjectMembers
            .Where(pm => pm.UserId == userId && pm.RemovedTime == null)
            .ToListAsync();

        foreach (var pm in activeMemberships)
            pm.RemovedTime = DateTime.UtcNow;

        // Soft-delete + scrub PII
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.Email = $"deleted-user-{user.Id}@sprintboard.invalid";
        user.PasswordHash = null;
        user.GoogleId = null;
        user.AvatarUrl = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return UserDeletionResult.Success();
    }
}

