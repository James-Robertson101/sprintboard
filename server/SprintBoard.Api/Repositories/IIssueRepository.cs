using SprintBoard.Api.Models;

namespace SprintBoard.Api.Repositories;

public interface IIssueRepository
{
    Task<Issue?> GetByIdAsync(int id);
    Task<List<Issue>> GetByProjectIdAsync(int projectId);
    Task<Issue> CreateAsync(Issue issue);
    Task<Issue> UpdateAsync(Issue issue);
    Task DeleteAsync(Issue issue);
}