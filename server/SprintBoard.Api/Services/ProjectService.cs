using SprintBoard.Api.DTOs;
using SprintBoard.Api.Models;
using SprintBoard.Api.Repositories;
using SprintBoard.Api.Exceptions;

namespace SprintBoard.Api.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;

    public ProjectService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository; //repository dependency injection
    }

    public async Task<ProjectDto> CreateProjectAsync(int userId, ProjectDto dto)
    {
        var project = new Project
        {
            Name = dto.Name,
            Description = dto.Description,
            Icon = dto.Icon
        }; //LINQ doesn't work with DTOs so we create a Project instance from the Dto

        var created = await _projectRepository.CreateProjectAsync(userId, project); //calling repository
        return MapToDto(created); //return DTO
    }

    public async Task<List<ProjectDto>> GetUserProjectsAsync(int userId)
    {   
        var projects = await _projectRepository.GetUserProjectsAsync(userId);
        if (projects == null)
            return new List<ProjectDto>();
        return projects.Select(MapToDto).ToList();
    }
    
    public async Task<ProjectDto> GetProjectByIdAsync(int projectId, int userId)
    {
        var project = await _projectRepository.GetProjectByIdAsync(projectId);

        if (project == null)
            throw new NotFoundException("Project not found");

        var isMember = project.ProjectMembers.Any(pm => pm.UserId == userId); //checking to see if user is a member
        if (!isMember)
            throw new NotFoundException("Project not found"); // don't reveal existence to non-members

        return MapToDto(project); //return ProjectDto
    }

public async Task DeleteProjectAsync(int userId, int projectId)
{
    var project = await _projectRepository.GetProjectByIdAsync(projectId)
        ?? throw new NotFoundException("Project could not be found");

    var member = project.ProjectMembers
        .FirstOrDefault(m => m.UserId == userId);

    if (member == null)
    {
        throw new ForbiddenException(
            "You do not have permission to delete this project");
    }

    if (member.ProjectRole != ProjectRole.Owner)
    {
        throw new ForbiddenException(
            "You do not have permission to delete this project");
    }

    await _projectRepository.DeleteProjectAsync(project);
}

    private static ProjectDto MapToDto(Project project) =>
        new(
            project.Name,
            project.Description,
            project.Icon
        );
}