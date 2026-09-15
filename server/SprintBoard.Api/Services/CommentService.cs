// Services/CommentService.cs
using SprintBoard.Api.DTOs;
using SprintBoard.Api.Exceptions;
using SprintBoard.Api.Models;
using SprintBoard.Api.Repositories;

namespace SprintBoard.Api.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _comments;
    private readonly IIssueRepository _issues;
    private readonly IProjectRepository _projects;

    public CommentService(
        ICommentRepository comments,
        IIssueRepository issues,
        IProjectRepository projects)
    {
        _comments = comments;
        _issues = issues;
        _projects = projects;
    }

    public async Task<List<CommentResponseDto>> GetCommentsAsync(int projectId, int issueId, int userId)
    {
        await EnsureIssueInProjectAsync(projectId, issueId);
        await GetMembershipAsync(projectId, userId);

        var comments = await _comments.GetByIssueIdAsync(issueId);
        return comments.Select(ToDto).ToList();
    }

    public async Task<CommentResponseDto> CreateCommentAsync(int projectId, int issueId, int userId, CreateCommentDto dto)
    {
        await EnsureIssueInProjectAsync(projectId, issueId);
        await GetMembershipAsync(projectId, userId);

        if (string.IsNullOrWhiteSpace(dto.Content))
        {
            throw new BadHttpRequestException("Comment content cannot be empty.");
        }

        var comment = new Comment
        {
            IssueId = issueId,
            AuthorId = userId,
            Content = dto.Content.Trim()
        };

        await _comments.AddAsync(comment);
        await _comments.SaveChangesAsync();

        var saved = await _comments.GetByIdAsync(comment.Id)
            ?? throw new InvalidOperationException("Comment was not persisted.");

        return ToDto(saved);
    }

    public async Task<CommentResponseDto> UpdateCommentAsync(int projectId, int issueId, int commentId, int userId, UpdateCommentDto dto)
    {
        await EnsureIssueInProjectAsync(projectId, issueId);
        var membership = await GetMembershipAsync(projectId, userId);

        var comment = await _comments.GetByIdAsync(commentId)
            ?? throw new NotFoundException("Comment not found.");

        if (comment.IssueId != issueId)
        {
            throw new NotFoundException("Comment not found.");
        }

        var isOwner = membership.ProjectRole == ProjectRole.Owner;
        var isAuthor = comment.AuthorId == userId;

        if (!isOwner && !isAuthor)
        {
            throw new ForbiddenException("You cannot edit this comment.");
        }

        if (string.IsNullOrWhiteSpace(dto.Content))
        {
            throw new BadHttpRequestException("Comment content cannot be empty.");
        }

        comment.Content = dto.Content.Trim();
        comment.UpdatedAt = DateTime.UtcNow;

        await _comments.SaveChangesAsync();

        return ToDto(comment);
    }

    public async Task DeleteCommentAsync(int projectId, int issueId, int commentId, int userId)
    {
        await EnsureIssueInProjectAsync(projectId, issueId);
        var membership = await GetMembershipAsync(projectId, userId);

        var comment = await _comments.GetByIdAsync(commentId)
            ?? throw new NotFoundException("Comment not found.");

        if (comment.IssueId != issueId)
        {
            throw new NotFoundException("Comment not found.");
        }

        var isOwner = membership.ProjectRole == ProjectRole.Owner;
        var isAuthor = comment.AuthorId == userId;

        if (!isOwner && !isAuthor)
        {
            throw new ForbiddenException("You cannot delete this comment.");
        }

        _comments.Remove(comment);
        await _comments.SaveChangesAsync();
    }

    private async Task EnsureIssueInProjectAsync(int projectId, int issueId)
    {
        var issue = await _issues.GetByIdAsync(issueId)
            ?? throw new NotFoundException("Issue not found.");

        if (issue.ProjectId != projectId)
        {
            throw new NotFoundException("Issue not found.");
        }
    }

    private async Task<ProjectMember> GetMembershipAsync(int projectId, int userId)
    {
        var project = await _projects.GetProjectByIdAsync(projectId)
            ?? throw new NotFoundException("Project not found");

        var member = project.ProjectMembers.FirstOrDefault(pm => pm.UserId == userId);

        if (member is null)
        {
            throw new NotFoundException("Project not found");
        }

        return member;
    }

    private static CommentResponseDto ToDto(Comment c) => new()
    {
        Id = c.Id,
        IssueId = c.IssueId,
        Content = c.Content,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
        Author = new UserSummaryDto(c.Author.Id, c.Author.Name, c.Author.AvatarUrl)
    };
}