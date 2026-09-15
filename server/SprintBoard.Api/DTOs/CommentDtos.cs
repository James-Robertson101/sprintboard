
namespace SprintBoard.Api.DTOs;

public class CreateCommentDto
{
    public string Content { get; set; } = string.Empty;
}

public class UpdateCommentDto
{
    public string Content { get; set; } = string.Empty;
}

public class CommentResponseDto
{
    public int Id { get; set; }
    public int IssueId { get; set; }
    public string Content { get; set; } = string.Empty;
    public UserSummaryDto Author { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}