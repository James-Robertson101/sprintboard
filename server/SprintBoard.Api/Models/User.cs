namespace SprintBoard.Api.Models;
public class User
{
  public int Id {get; set;}
  public string Name { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public UserRole Role {get; set;} = UserRole.User;
  public string? PasswordHash { get; set; } // null if they used Google
  public string? GoogleId { get; set; }     // null if they used email/password
  public string? AvatarUrl { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
  public List<ProjectMember> ProjectMembers {get; set;} = new();

}