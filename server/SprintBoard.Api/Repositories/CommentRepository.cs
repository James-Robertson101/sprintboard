// Repositories/CommentRepository.cs
using Microsoft.EntityFrameworkCore;
using SprintBoard.Api.Data;
using SprintBoard.Api.Models;

namespace SprintBoard.Api.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly AppDbContext _db;

    public CommentRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Comment>> GetByIssueIdAsync(int issueId)
    {
        return await _db.Comments
            .IgnoreQueryFilters()
            .Include(c => c.Author)
            .Where(c => c.IssueId == issueId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Comment?> GetByIdAsync(int commentId)
    {
        return await _db.Comments
            .IgnoreQueryFilters()
            .Include(c => c.Author)
            .Include(c => c.Issue)
            .FirstOrDefaultAsync(c => c.Id == commentId);
    }

    public async Task AddAsync(Comment comment)
    {
        await _db.Comments.AddAsync(comment);
    }

    public void Remove(Comment comment)
    {
        _db.Comments.Remove(comment);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}