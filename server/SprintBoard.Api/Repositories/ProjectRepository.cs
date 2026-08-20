using SprintBoard.Api.Data;
using SprintBoard.Api.Models;
using SprintBoard.Api.Repositories;
using SprintBoard.API.Repositories;

namespace SprintBoard.Api.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _db;

    public ProjectRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Project> CreateProjectAsync(int userId, Project project)
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
}