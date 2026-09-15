
using SprintBoard.Api.Models;

namespace SprintBoard.Api.Repositories;

public interface ICommentRepository
{
    Task<List<Comment>> GetByIssueIdAsync(int issueId);
    Task<Comment?> GetByIdAsync(int commentId);
    Task AddAsync(Comment comment);
    void Remove(Comment comment);
    Task SaveChangesAsync();
}