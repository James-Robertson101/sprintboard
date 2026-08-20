namespace SprintBoard.Api.Models;
public class ProjectMember
{
  public int Id {get; set; }
  public ProjectRole ProjectRole {get; set; } = ProjectRole.Member;
  public int ProjectId {get; set; }
  public Project Project {get; set; }  = null!;
  public int UserId {get; set; }
  public User User {get; set; }  = null!;
  public DateTime JoinTime {get;set; } = DateTime.UtcNow;
  public DateTime? RemovedTime {get;set; }
  public User? InvitedByUser {get;set; }
  public int? InvitedByUserId {get; set;}
  public uint RowVersion { get; set; }
}