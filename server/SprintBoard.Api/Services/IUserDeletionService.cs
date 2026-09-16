namespace SprintBoard.Api.Services;
using SprintBoard.Api.DTOs;
public interface IUserDeletionService
{
    Task<UserDeletionResult> DeleteUserAsync(int userId, int requestingUserId);
}