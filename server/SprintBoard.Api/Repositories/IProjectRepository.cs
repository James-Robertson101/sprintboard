namespace SprintBoard.API.Repositories;
using SprintBoard.Api.DTOs;
using SprintBoard.Api.Models;

public interface IProjectRepository
{
  Task<Project> CreateProjectAsync(int userId, Project project);
  //public Task<List<ProjectDto>> getUserProjectsAsync(int UserId);
  //public Task DeleteProjectAsync(ProjectDto project);
  //public Task UpdateProjectAsync(ProjectDto project);
  //public Task<ProjectDto> FindByID(int id);
}