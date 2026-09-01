using Microsoft.EntityFrameworkCore;
using SprintBoard.Api.Data;
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
}