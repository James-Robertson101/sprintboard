using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SprintBoard.Api.DTOs;
using SprintBoard.Api.Services;

namespace SprintBoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ProjectDto>> CreateProject(ProjectDto projectDto)
    {
        try
        {
            var userId = User.GetUserId();
            var response = await _projectService.CreateProjectAsync(userId, projectDto);
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return StatusCode(500, "An error occurred while creating the project.");
        }
    }
}