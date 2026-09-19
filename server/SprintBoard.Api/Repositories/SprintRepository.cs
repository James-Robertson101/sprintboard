using Microsoft.EntityFrameworkCore;
using SprintBoard.Api.Data;
using SprintBoard.Api.Models;

namespace SprintBoard.Api.Repositories;

public class SprintRepository : ISprintRepository
{
    private readonly AppDbContext _context;

    public SprintRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Sprint>> GetByProjectIdAsync(int projectId)
    {
        return await _context.Sprints
            .Where(s => s.ProjectId == projectId)
            .Include(s => s.Issues)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync();
    }

    public async Task<Sprint?> GetByIdAsync(
        int projectId,
        int sprintId)
    {
        return await _context.Sprints
            .Include(s => s.Issues)
            .FirstOrDefaultAsync(s =>
                s.Id == sprintId &&
                s.ProjectId == projectId);
    }

    public async Task<bool> HasActiveSprintAsync(int projectId)
    {
        return await _context.Sprints
            .AnyAsync(s =>
                s.ProjectId == projectId &&
                s.Status == SprintStatus.Active);
    }

    public async Task AddAsync(Sprint sprint)
    {
        await _context.Sprints.AddAsync(sprint);
    }

    public void Update(Sprint sprint)
    {
        _context.Sprints.Update(sprint);
    }

    public void Delete(Sprint sprint)
    {
        _context.Sprints.Remove(sprint);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<Sprint?> GetActiveSprintAsync(int projectId)
    {
        return await _context.Sprints
        .FirstOrDefaultAsync(s =>
            s.ProjectId == projectId &&
            s.Status == SprintStatus.Active);}

}