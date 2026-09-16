using Microsoft.EntityFrameworkCore;
using SprintBoard.Api.Data;
using SprintBoard.Api.DTOs;
using SprintBoard.Api.Models;

namespace SprintBoard.Api.Repositories;

public class IssueRepository : IIssueRepository
{
    private readonly AppDbContext _context;

    public IssueRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Issue?> GetByIdAsync(int id)
    {
        return await _context.Issues
            .Include(i => i.Assignee)
            .Include(i => i.CreatedBy)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<List<Issue>> GetCurrentIssues(int projectId)
    {
        return await _context.Issues
        .Include(i => i.Assignee)
        .Include(i => i.CreatedBy)
        .Include(i => i.Sprint)
        .Where(i => i.ProjectId == projectId)
        .Where(i => i.Sprint != null && i.Sprint.Status == SprintStatus.Active)
        .ToListAsync();
    }

    public async Task<List<Issue>> GetByProjectIdAsync(int projectId)
    {
        return await _context.Issues
            .Include(i => i.Assignee)
            .Include(i => i.CreatedBy)
            .Where(i => i.ProjectId == projectId)
            .OrderBy(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<Issue> CreateAsync(Issue issue)
    {
        _context.Issues.Add(issue);
        await _context.SaveChangesAsync();

        // Reload with navigation properties populated (Assignee may be null,
        // CreatedBy should always be set) so the mapped DTO has full data.
        return await GetByIdAsync(issue.Id)
            ?? throw new InvalidOperationException("Issue was not found immediately after creation.");
    }

    public async Task<Issue> UpdateAsync(Issue issue)
    {
        _context.Issues.Update(issue);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(issue.Id)
            ?? throw new InvalidOperationException("Issue was not found immediately after update.");
    }

    public async Task DeleteAsync(Issue issue)
    {
        _context.Issues.Remove(issue);
        await _context.SaveChangesAsync();
    }

    public async Task<List<IssueResponseDto>> GetBacklogAsync(int projectId)
{
    return await _context.Issues
        .Where(i => i.ProjectId == projectId && i.SprintId == null)
        .Select(i => new IssueResponseDto(
            i.Id,
            i.Name,
            i.Description,
            i.Priority,
            i.Status,
            i.Assignee == null
                ? null
                : new AssigneeDto(
                    i.Assignee.Id,
                    i.Assignee.Name,
                    i.Assignee.AvatarUrl
                ),
            i.CreatedAt,
            i.UpdatedAt
        ))
        .ToListAsync();
}
}