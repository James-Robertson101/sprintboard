using SprintBoard.Api.Models;
using SprintBoard.Api.Repositories;
using SprintBoard.Api.DTOs;

namespace SprintBoard.Api.Services;

public class UserDeletionService : IUserDeletionService
{
    private readonly IUserRepository _userRepository;

    public UserDeletionService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDeletionResult> DeleteMyAccountAsync(int userId)
    {
        return await DeleteAccountInternalAsync(userId);
    }

    public async Task<UserDeletionResult> DeleteUserAsync(
        int userId,
        int requestingUserId)
    {
        if (userId == requestingUserId)
        {
            return UserDeletionResult.Conflict(
                "Use the delete-my-account operation to delete your own account.");
        }

        return await DeleteAccountInternalAsync(userId);
    }

    private async Task<UserDeletionResult> DeleteAccountInternalAsync(
        int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
            return UserDeletionResult.NotFound();

        var isSoleOwner =
            await _userRepository.IsSoleOwnerOfAnyProjectAsync(userId);

        if (isSoleOwner)
        {
            return UserDeletionResult.Conflict(
                "User is the sole owner of one or more projects. " +
                "Transfer ownership before deleting.");
        }

        await _userRepository.RemoveActiveProjectMembershipsAsync(userId);

        var now = DateTime.UtcNow;

        user.IsDeleted = true;
        user.DeletedAt = now;
        user.Email = $"deleted-user-{user.Id}@sprintboard.invalid";
        user.PasswordHash = null;
        user.GoogleId = null;
        user.AvatarUrl = null;
        user.UpdatedAt = now;

        await _userRepository.UpdateAsync(user);

        return UserDeletionResult.Success();
    }
}