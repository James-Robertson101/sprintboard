

namespace SprintBoard.Api.Models;
public class Project
{
  public int Id {get; set; }
  public string Name {get; set; } = string.Empty;
  public List<ProjectMember> ProjectMembers {get; set; } = new();

}