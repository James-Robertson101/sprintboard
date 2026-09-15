namespace SprintBoard.Api.Models;

public class Sprint
{
    public int Id { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public string? Goal { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public SprintStatus Status { get; set; } = SprintStatus.Planned;

    public List<Issue> Issues { get; set; } = new();
}