namespace SprintBoard.Api.Services;
using SprintBoard.Api.DTOs;
public interface ISprintService
{
    Task<List<SprintResponseDto>> GetSprintsAsync(
        int projectId,
        int currentUserId);

    Task<SprintResponseDto> GetSprintAsync(
        int projectId,
        int sprintId,
        int currentUserId);

    Task<SprintResponseDto> CreateSprintAsync(
        int projectId,
        int currentUserId,
        CreateSprintDto dto);

    Task<SprintResponseDto> UpdateSprintAsync(
        int projectId,
        int sprintId,
        int currentUserId,
        UpdateSprintDto dto);

    Task<SprintResponseDto> StartSprintAsync(
        int projectId,
        int sprintId,
        int currentUserId);

    Task<SprintResponseDto> CompleteSprintAsync(
        int projectId,
        int sprintId,
        int currentUserId);

    Task DeleteSprintAsync(
        int projectId,
        int sprintId,
        int currentUserId);
}