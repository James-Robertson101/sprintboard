namespace SprintBoard.Api.Models;

public class Issue
{
    public int Id { get; set; }

    // Project relationship
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    // Basic issue information
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // State
    public IssueStatus Status {get; set;}

    // Priority
    public Priority Priority { get; set; } = Priority.Medium;

    // Assignment
    public int? AssigneeId { get; set; }
    public User? Assignee { get; set; }

    // Creation
    public int CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Optional lifecycle information
    public DateTime? UpdatedAt { get; set; }
}