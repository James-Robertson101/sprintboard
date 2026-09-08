using SprintBoard.Api.DTOs;
using SprintBoard.Api.Exceptions;
using SprintBoard.Api.Models;
using SprintBoard.Api.Repositories;

namespace SprintBoard.Api.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;

    public ProjectService(
        IProjectRepository projectRepository,
        IUserRepository userRepository)
    {
        _projectRepository = projectRepository;
        _userRepository = userRepository;
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
        int userId, string? search)
    {
        var projects = await _projectRepository.GetUserProjectsAsync(
            userId, search);

        return projects
            .Select(MapToResponseDto)
            .ToList();
    }

    public async Task<ProjectResponseDto> GetProjectByIdAsync(
        int projectId,
        int userId)
    {
        var project = await GetProjectAndVerifyMembershipAsync(projectId, userId);
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

    // --- Members ---

    public async Task<List<ProjectMemberDto>> GetMembersAsync(int projectId, int userId)
    {
        var project = await GetProjectAndVerifyMembershipAsync(projectId, userId);

        return project.ProjectMembers
            .Select(MapToMemberDto)
            .ToList();
    }

    public async Task<ProjectMemberDto> AddMemberAsync(int projectId, int ownerId, AddMemberDto dto)
    {
        var project = await GetProjectAndVerifyOwnerAsync(projectId, ownerId);

        var userToAdd = await _userRepository.FindByEmailAsync(dto.Email)
            ?? throw new NotFoundException("No user found with that email.");

        var alreadyMember = project.ProjectMembers.Any(m => m.UserId == userToAdd.Id);
        if (alreadyMember)
        {
            throw new ConflictException("This user is already a member of the project.");
        }

        var member = new ProjectMember
        {
            ProjectId = projectId,
            UserId = userToAdd.Id,
            User = userToAdd,
            ProjectRole = ProjectRole.Member,
            JoinTime = DateTime.UtcNow,
            InvitedByUserId = ownerId
        };

        project.ProjectMembers.Add(member);
        await _projectRepository.UpdateProjectAsync(project);

        return MapToMemberDto(member);
    }

    public async Task RemoveMemberAsync(int projectId, int ownerId, int targetUserId)
    {
        var project = await GetProjectAndVerifyOwnerAsync(projectId, ownerId);

        var member = project.ProjectMembers.FirstOrDefault(m => m.UserId == targetUserId)
            ?? throw new NotFoundException("This user is not a member of the project.");

        if (member.ProjectRole == ProjectRole.Owner)
        {
            throw new ForbiddenException(
                "The project owner cannot be removed. Delete the project or transfer ownership instead.");
        }

        member.RemovedTime = DateTime.UtcNow;
        await _projectRepository.UpdateProjectAsync(project);
    }

    public async Task<List<UserSummaryDto>> GetAvailableUsersAsync(int projectId, int userId, string? search)
{
    var project = await GetProjectAndVerifyMembershipAsync(projectId, userId);

    var existingMemberIds = project.ProjectMembers
        .Select(m => m.UserId)
        .ToHashSet();

    var candidates = await _userRepository.SearchUsersAsync(search);

    return candidates
        .Where(u => !existingMemberIds.Contains(u.Id))
        .Select(u => new UserSummaryDto(u.Id, u.Name, u.AvatarUrl))
        .ToList();
}
    // --- Helpers ---

    private async Task<Project> GetProjectAndVerifyMembershipAsync(int projectId, int userId)
    {
        var project = await _projectRepository.GetProjectByIdAsync(projectId);
        if (project is null)
        {
            throw new NotFoundException("Project not found");
        }

        var isMember = project.ProjectMembers.Any(pm => pm.UserId == userId);

        // Don't reveal whether the project exists to non-members.
        if (!isMember)
        {
            throw new NotFoundException("Project not found");
        }

        return project;
    }

    private async Task<Project> GetProjectAndVerifyOwnerAsync(int projectId, int userId)
    {
        var project = await GetProjectAndVerifyMembershipAsync(projectId, userId);

        var member = project.ProjectMembers.First(m => m.UserId == userId);
        if (member.ProjectRole != ProjectRole.Owner)
        {
            throw new ForbiddenException("Only the project owner can do this.");
        }

        return project;
    }


    private static ProjectMemberDto MapToMemberDto(ProjectMember member)
    {
        return new ProjectMemberDto(
            member.UserId,
            member.User.Name,
            member.User.AvatarUrl,
            member.ProjectRole,
            member.JoinTime
        );
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