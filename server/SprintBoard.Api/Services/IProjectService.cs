using SprintBoard.Api.DTOs;

namespace SprintBoard.Api.Services;

public interface IProjectService
{
    Task<ProjectResponseDto> CreateProjectAsync(
        int userId,
        ProjectDto dto);

    Task<List<ProjectResponseDto>> GetUserProjectsAsync(
        int userId, string? search);

    Task<ProjectResponseDto> GetProjectByIdAsync(
        int projectId,
        int userId);

    Task DeleteProjectAsync(
        int userId,
        int projectId);

    Task<ProjectResponseDto> UpdateProjectAsync(
        int userId,
        int projectId,
        ProjectDto dto);
}