using Microsoft.AspNetCore.Mvc;
using SprintBoard.Api.DTOs;
using SprintBoard.Api.Models;

public interface IProjectService
{
  Task<ProjectDto> CreateProjectAsync(int userId, ProjectDto dto);
  Task<List<ProjectDto>> GetUserProjectsAsync(int UserId);
  //public Task DeleteProjectAsync(ProjectDto project);
  //public Task UpdateProjectAsync(ProjectDto project);
  //public Task<ProjectDto> FindByID(int id);
}