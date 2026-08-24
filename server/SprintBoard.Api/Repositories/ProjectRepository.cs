using Microsoft.EntityFrameworkCore;
using SprintBoard.Api.Data;
using SprintBoard.Api.Models;

namespace SprintBoard.Api.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _db;

    public ProjectRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Project> CreateProjectAsync(
        int userId,
        Project project)
    {
        project.ProjectMembers.Add(new ProjectMember
        {
            UserId = userId,
            ProjectRole = ProjectRole.Owner,
            JoinTime = DateTime.UtcNow
        });

        _db.Projects.Add(project);

        await _db.SaveChangesAsync();

        return project;
    }

   public async Task<List<Project>> GetUserProjectsAsync(
    int userId,
    string? search)
{
    var query = _db.Projects
        .Include(p => p.ProjectMembers)
        .Where(p => p.ProjectMembers.Any(pm => pm.UserId == userId));

    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(p => p.Name.Contains(search));
    }

    return await query.ToListAsync();
}

    public async Task<Project?> GetProjectByIdAsync(
        int projectId)
    {
        return await _db.Projects
            .Include(p => p.ProjectMembers)
            .FirstOrDefaultAsync(p => p.Id == projectId);
    }

    public async Task DeleteProjectAsync(Project project)
    {
        _db.Projects.Remove(project);

        await _db.SaveChangesAsync();
    }

    public async Task<Project> UpdateProjectAsync(
        Project project)
    {
        await _db.SaveChangesAsync();

        return project;
    }
}