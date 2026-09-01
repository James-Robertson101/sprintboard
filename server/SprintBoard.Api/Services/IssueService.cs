using SprintBoard.Api.DTOs;
using SprintBoard.Api.Exceptions;
using SprintBoard.Api.Models;
using SprintBoard.Api.Repositories;

namespace SprintBoard.Api.Services;

public class IssueService : IIssueService
{
    private readonly IIssueRepository _issueRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;

    public IssueService(
        IIssueRepository issueRepository,
        IProjectRepository projectRepository,
        IUserRepository userRepository)
    {
        _issueRepository = issueRepository;
        _projectRepository = projectRepository;
        _userRepository = userRepository;
    }

    public async Task<List<IssueResponseDto>> GetIssuesForProjectAsync(int projectId, int userId)
    {
        var project = await GetProjectAndVerifyMembershipAsync(projectId, userId);

        var issues = await _issueRepository.GetByProjectIdAsync(project.Id);
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

        var issue = new Issue
        {
            ProjectId = projectId,
            Name = dto.Name,
            Description = dto.Description,
            Priority = dto.Priority,
            Status = IssueStatus.Todo,
            AssigneeId = assignee?.Id,
            Assignee = assignee,
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _issueRepository.CreateAsync(issue);
        return MapToDto(created);
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
        return MapToDto(updated);
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