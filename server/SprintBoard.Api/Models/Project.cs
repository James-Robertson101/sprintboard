

namespace SprintBoard.Api.Models;
public class Project
{
  public int Id {get; set; }
  public string Name {get; set; } = string.Empty;
  public string? Description {get;set;}
  public string? Icon {get;set;}
  public List<ProjectMember> ProjectMembers {get; set; } = new();

}