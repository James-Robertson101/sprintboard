namespace SprintBoard.Api.Repositories;
using SprintBoard.Api.DTOs;
using SprintBoard.Api.Models;

public interface IProjectRepository
{
  Task<Project> CreateProjectAsync(int userId, Project project);
  Task<List<Project>> GetUserProjectsAsync(int UserId);
  Task<Project?> GetProjectByIdAsync(int projectId);
  //public Task DeleteProjectAsync(ProjectDto project);
  //public Task UpdateProjectAsync(ProjectDto project);
  //public Task<ProjectDto> FindByID(int id);
}