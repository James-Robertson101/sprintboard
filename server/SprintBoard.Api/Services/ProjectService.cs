using SprintBoard.Api.DTOs;
using SprintBoard.Api.Models;
using SprintBoard.Api.Repositories;
using SprintBoard.API.Repositories;

namespace SprintBoard.Api.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;

    public ProjectService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<ProjectDto> CreateProjectAsync(int userId, ProjectDto dto)
    {
        var project = new Project
        {
            Name = dto.Name,
            Description = dto.Description,
            Icon = dto.Icon
        };

        var created = await _projectRepository.CreateProjectAsync(userId, project);
        return MapToDto(created);
    }

    private static ProjectDto MapToDto(Project project) =>
        new(
            project.Name,
            project.Description,
            project.Icon
        );
}