using SprintBoard.Api.Models;

namespace SprintBoard.Api.Repositories;

public interface ISprintRepository
{
    Task<List<Sprint>> GetByProjectIdAsync(int projectId);

    Task<Sprint?> GetByIdAsync(int projectId, int sprintId);

    Task<bool> HasActiveSprintAsync(int projectId);

    Task AddAsync(Sprint sprint);

    void Update(Sprint sprint);

    void Delete(Sprint sprint);

    Task SaveChangesAsync();
    Task<Sprint?> GetActiveSprintAsync(int projectId);
}