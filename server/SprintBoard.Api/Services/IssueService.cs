using SprintBoard.Api.DTOs;
using SprintBoard.Api.Exceptions;
using SprintBoard.Api.Models;
using SprintBoard.Api.Repositories;
using Microsoft.AspNetCore.SignalR;
using SprintBoard.Api.Hubs;

namespace SprintBoard.Api.Services;

public class IssueService : IIssueService
{
    private readonly IIssueRepository _issueRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;
    private readonly ISprintRepository _sprintRepository;
    private readonly IHubContext<SprintBoardHub> _hubContext;

    public IssueService(
        IIssueRepository issueRepository,
        IProjectRepository projectRepository,
        IUserRepository userRepository,
        ISprintRepository sprintRepository,
        IHubContext<SprintBoardHub> hubContext
        )
    {
        _issueRepository = issueRepository;
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _sprintRepository = sprintRepository;
        _hubContext = hubContext;
    }

    public async Task<List<IssueResponseDto>> GetIssuesForProjectAsync(int projectId, int userId)
    {
        var project = await GetProjectAndVerifyMembershipAsync(projectId, userId);

        var issues = await _issueRepository.GetByProjectIdAsync(project.Id);
        return issues.Select(MapToDto).ToList();
    }

    public async Task<List<IssueResponseDto>> GetCurrentIssues(int projectId, int userId)
    {
        var project = await GetProjectAndVerifyMembershipAsync(projectId, userId);

        var issues = await _issueRepository.GetCurrentIssues(project.Id);
        return issues.Select(MapToDto).ToList();
    }

    public async Task<IssueResponseDto> GetIssueByIdAsync(int projectId, int issueId, int userId)
    {
        await GetProjectAndVerifyMembershipAsync(projectId, userId);

        var issue = await _issueRepository.GetByIdAsync(issueId);
        if (issue is null || issue.ProjectId != projectId)
        {
            throw new NotFoundException("Issue not found.");
        }

        return MapToDto(issue);
    }

     public async Task<IssueResponseDto> CreateIssueAsync(int projectId, int userId, CreateIssueDto dto)
    {
        await GetProjectAndVerifyMembershipAsync(projectId, userId);

        User? assignee = null;
        if (dto.AssigneeId is not null)
        {
            assignee = await GetAssigneeAndVerifyMembershipAsync(projectId, dto.AssigneeId.Value);
        }
        var activeSprint = await _sprintRepository.GetActiveSprintAsync(projectId);
        var issue = new Issue
        {
            ProjectId = projectId,
            SprintId = activeSprint?.Id,
            Name = dto.Name,
            Description = dto.Description,
            Priority = dto.Priority,
            Status = dto.Status,
            AssigneeId = assignee?.Id,
            Assignee = assignee,
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _issueRepository.CreateAsync(issue);
        var response = MapToDto(created);
        Console.WriteLine(
        $"[SignalR] Sending IssueCreated to project-{projectId}"
);
        await _hubContext.Clients
        .Group($"project-{projectId}")
        .SendAsync("IssueCreated", response);

        return response;

    }


    public async Task<IssueResponseDto> UpdateIssueAsync(int projectId, int issueId, int userId, UpdateIssueDto dto)
    {
        await GetProjectAndVerifyMembershipAsync(projectId, userId);

        var issue = await _issueRepository.GetByIdAsync(issueId);
        if (issue is null || issue.ProjectId != projectId)
        {
            throw new NotFoundException("Issue not found.");
        }

        User? assignee = null;
        if (dto.AssigneeId is not null)
        {
            assignee = await GetAssigneeAndVerifyMembershipAsync(projectId, dto.AssigneeId.Value);
        }

        issue.Name = dto.Name;
        issue.Description = dto.Description;
        issue.Priority = dto.Priority;
        issue.Status = dto.Status;
        issue.AssigneeId = assignee?.Id;
        issue.Assignee = assignee;
        issue.UpdatedAt = DateTime.UtcNow;

        var updated = await _issueRepository.UpdateAsync(issue);
        var response = MapToDto(updated);
        await _hubContext.Clients
        .Group($"project-{projectId}")
        .SendAsync("IssueUpdated", response);

        return response;
    }

    public async Task DeleteIssueAsync(int projectId, int issueId, int userId)
    {
        await GetProjectAndVerifyMembershipAsync(projectId, userId);

        var issue = await _issueRepository.GetByIdAsync(issueId);
        if (issue is null || issue.ProjectId != projectId)
        {
            throw new NotFoundException("Issue not found.");
        }

        await _issueRepository.DeleteAsync(issue);
        await _hubContext.Clients
        .Group($"project-{projectId}")
        .SendAsync("IssueDeleted", issueId);
    }

    public async Task<List<IssueResponseDto>> GetBacklogAsync(int projectId, int currentUserId)
{
    await GetProjectAndVerifyMembershipAsync(
        projectId,
        currentUserId);

    var issues = await _issueRepository.GetBacklogAsync(projectId);
    return issues;
}

    public async Task<IssueResponseDto> AssignIssueToSprintAsync(
    int projectId,
    int issueId,
    int userId,
    AssignIssueToSprintDto dto)
{
    // Verify the current user can access this project.
    await GetProjectAndVerifyMembershipAsync(projectId, userId);

    // Make sure the issue exists and belongs to this project.
    var issue = await _issueRepository.GetByIdAsync(issueId);

    if (issue is null || issue.ProjectId != projectId)
    {
        throw new NotFoundException("Issue not found.");
    }

    // null means "move to backlog".
    if (dto.SprintId is null)
    {
        issue.SprintId = null;
    }
    else
    {
        // Verify that the sprint exists and belongs to this project.
        var sprint = await _sprintRepository.GetByIdAsync(
            projectId,
            dto.SprintId.Value);

        if (sprint is null)
        {
            throw new NotFoundException("Sprint not found.");
        }

        issue.SprintId = sprint.Id;
    }

    issue.UpdatedAt = DateTime.UtcNow;

    var updated = await _issueRepository.UpdateAsync(issue);

    return MapToDto(updated);
}
    // --- Helpers ---
    // NOTE: authorization rules below are a starting point (any project member
    // can create/edit/delete issues). Tighten these if you want role-specific
    // behaviour, e.g. only certain ProjectRoles can delete issues.

    private async Task<Project> GetProjectAndVerifyMembershipAsync(int projectId, int userId)
    {
        var project = await _projectRepository.GetProjectByIdAsync(projectId);
        if (project is null)
        {
            throw new NotFoundException("Project not found.");
        }

        var isMember = project.ProjectMembers.Any(m => m.UserId == userId);
        if (!isMember)
        {
            throw new NotFoundException("Project not found.");
        }

        return project;
    }

    private async Task<User> GetAssigneeAndVerifyMembershipAsync(int projectId, int assigneeId)
    {
        var user = await _userRepository.GetByIdAsync(assigneeId);
        if (user is null)
        {
            throw new NotFoundException("Assignee not found.");
        }

        var project = await _projectRepository.GetProjectByIdAsync(projectId);
        var isMember = project!.ProjectMembers.Any(m => m.UserId == assigneeId);
        if (!isMember)
        {
            throw new ForbiddenException("Assignee is not a member of this project.");
        }

        return user;
    }

    private static IssueResponseDto MapToDto(Issue issue)
    {
        AssigneeDto? assigneeDto = issue.Assignee is null
            ? null
            : new AssigneeDto(issue.Assignee.Id, issue.Assignee.Name, issue.Assignee.AvatarUrl);

        return new IssueResponseDto(
            issue.Id,
            issue.Name,
            issue.Description,
            issue.Priority,
            issue.Status,
            assigneeDto,
            issue.CreatedAt,
            issue.UpdatedAt
        );
    }
}