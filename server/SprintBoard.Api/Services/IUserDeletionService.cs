using SprintBoard.Api.DTOs;

public interface IUserDeletionService
{
    Task<UserDeletionResult> DeleteUserAsync(
        int userId,
        int requestingUserId);

    Task<UserDeletionResult> DeleteMyAccountAsync(
        int userId);
}