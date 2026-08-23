using SprintBoard.Api.DTOs;
using SprintBoard.Api.Exceptions;
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

    public async Task<ProjectResponseDto> CreateProjectAsync(
        int userId,
        ProjectDto dto)
    {
        var project = new Project
        {
            Name = dto.Name,
            Description = dto.Description,
            Icon = dto.Icon
        };

        var created = await _projectRepository.CreateProjectAsync(
            userId,
            project);

        return MapToResponseDto(created);
    }

    public async Task<List<ProjectResponseDto>> GetUserProjectsAsync(
        int userId)
    {
        var projects = await _projectRepository.GetUserProjectsAsync(
            userId);

        return projects
            .Select(MapToResponseDto)
            .ToList();
    }

    public async Task<ProjectResponseDto> GetProjectByIdAsync(
        int projectId,
        int userId)
    {
        var project = await _projectRepository.GetProjectByIdAsync(
            projectId);

        if (project == null)
        {
            throw new NotFoundException("Project not found");
        }

        var isMember = project.ProjectMembers
            .Any(pm => pm.UserId == userId);

        // Don't reveal whether the project exists
        // to users who aren't members.
        if (!isMember)
        {
            throw new NotFoundException("Project not found");
        }

        return MapToResponseDto(project);
    }

    public async Task DeleteProjectAsync(
        int userId,
        int projectId)
    {
        var project = await _projectRepository.GetProjectByIdAsync(
            projectId)
            ?? throw new NotFoundException(
                "Project could not be found");

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

    public async Task<ProjectResponseDto> UpdateProjectAsync(
        int userId,
        int projectId,
        ProjectDto dto)
    {
        var project = await _projectRepository.GetProjectByIdAsync(
            projectId)
            ?? throw new NotFoundException(
                "Project could not be found");

        var member = project.ProjectMembers
            .FirstOrDefault(m => m.UserId == userId);

        if (member == null)
        {
            throw new ForbiddenException(
                "You do not have permission to update this project");
        }

        project.Name = dto.Name;
        project.Description = dto.Description;
        project.Icon = dto.Icon;

        var updated = await _projectRepository.UpdateProjectAsync(
            project);

        return MapToResponseDto(updated);
    }

    private static ProjectResponseDto MapToResponseDto(
        Project project)
    {
        return new ProjectResponseDto(
            project.Id,
            project.Name,
            project.Description,
            project.Icon
        );
    }
}