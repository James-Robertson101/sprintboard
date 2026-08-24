using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SprintBoard.Api.DTOs;
using SprintBoard.Api.Exceptions;
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
    public async Task<ActionResult<ProjectResponseDto>> CreateProject(
        ProjectDto projectDto)
    {
        try
        {
            var userId = User.GetUserId();

            var response = await _projectService.CreateProjectAsync(
                userId,
                projectDto);

            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);

            return StatusCode(
                500,
                "An error occurred while creating the project.");
        }
    }

    [Authorize]
    [HttpGet("MyProjects")]
    public async Task<ActionResult<List<ProjectResponseDto>>> GetUserProjectsAsync([FromQuery] string? search)
    {
        try
        {
            var userId = User.GetUserId();

            var response = await _projectService.GetUserProjectsAsync(userId,search);

            return Ok(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);

            return StatusCode(
                500,
                "An error occurred whilst fetching projects.");
        }
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectResponseDto>> GetProjectById(int id)
    {
        try
        {
            var userId = User.GetUserId();

            var response = await _projectService.GetProjectByIdAsync(
                id,
                userId);

            return Ok(response);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);

            return StatusCode(
                500,
                "An error occurred while fetching the project.");
        }
    }

    [Authorize]
    [HttpDelete("{projectId:int}")]
    public async Task<ActionResult> DeleteProjectAsync(int projectId)
    {
        try
        {
            var userId = User.GetUserId();

            await _projectService.DeleteProjectAsync(
                userId,
                projectId);

            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (ForbiddenException)
        {
            return Forbid();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);

            return StatusCode(
                500,
                "An error occurred while deleting the project.");
        }
    }

    [Authorize]
    [HttpPut("{projectId:int}")]
    public async Task<ActionResult<ProjectResponseDto>> UpdateProjectAsync(
        int projectId,
        ProjectDto projectDto)
    {
        try
        {
            var userId = User.GetUserId();

            var response = await _projectService.UpdateProjectAsync(
                userId,
                projectId,
                projectDto);

            return Ok(response);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (ForbiddenException)
        {
            return Forbid();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);

            return StatusCode(
                500,
                "An error occurred while updating the project.");
        }
    }
}