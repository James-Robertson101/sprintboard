// Services/ICommentService.cs
using SprintBoard.Api.DTOs;

namespace SprintBoard.Api.Services;

public interface ICommentService
{
    Task<List<CommentResponseDto>> GetCommentsAsync(int projectId, int issueId, int userId);
    Task<CommentResponseDto> CreateCommentAsync(int projectId, int issueId, int userId, CreateCommentDto dto);
    Task<CommentResponseDto> UpdateCommentAsync(int projectId, int issueId, int commentId, int userId, UpdateCommentDto dto);
    Task DeleteCommentAsync(int projectId, int issueId, int commentId, int userId);
}