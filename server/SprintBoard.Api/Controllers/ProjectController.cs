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
    [HttpPost("CreateProject")]
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
    [Authorize]
    [HttpGet("MyProjects")]
    public async Task<ActionResult<List<ProjectDto>>> GetUserProjectsAsync()
    {
        try
        {
            var userId = User.GetUserId();
            var response = await _projectService.GetUserProjectsAsync(userId);
            Console.WriteLine(response);
            return response;
        }
        catch(Exception e)
        {
            Console.WriteLine(e.Message);
            return StatusCode(500, "an Error occured whilst fetching projects.");
        }
    }
}