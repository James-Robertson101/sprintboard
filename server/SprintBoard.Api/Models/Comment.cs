// Models/Comment.cs
namespace SprintBoard.Api.Models;

public class Comment
{
    public int Id { get; set; }

    public int IssueId { get; set; }
    public Issue Issue { get; set; } = null!;

    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}