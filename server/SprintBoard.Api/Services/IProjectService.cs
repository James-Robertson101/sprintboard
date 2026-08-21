using Microsoft.AspNetCore.Mvc;
using SprintBoard.Api.DTOs;
using SprintBoard.Api.Models;

public interface IProjectService
{
  Task<ProjectDto> CreateProjectAsync(int userId, ProjectDto dto);
  Task<List<ProjectDto>> GetUserProjectsAsync(int UserId);
  Task<ProjectDto> GetProjectByIdAsync(int projectId, int userId);
  //Task DeleteProjectAsync(int userId, int projectId);
  //Task UpdateProjectAsync(ProjectDto project);
  //Task<ProjectDto> FindByID(int id);
}