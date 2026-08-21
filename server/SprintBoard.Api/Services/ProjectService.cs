using SprintBoard.Api.DTOs;
using SprintBoard.Api.Models;
using SprintBoard.Api.Repositories;


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
    public async Task<List<ProjectDto>> GetUserProjectsAsync(int userId)
    {   
        var projects = await _projectRepository.GetUserProjectsAsync(userId);
        if (projects == null)
            return new List<ProjectDto>();
        return projects.Select(MapToDto).ToList();
    }

    private static ProjectDto MapToDto(Project project) =>
        new(
            project.Name,
            project.Description,
            project.Icon
        );
}