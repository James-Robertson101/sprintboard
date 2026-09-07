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
    public async Task<ActionResult<ProjectResponseDto>> CreateProject(ProjectDto projectDto)
    {
        var userId = User.GetUserId();
        var response = await _projectService.CreateProjectAsync(userId, projectDto);
        return Ok(response);
    }

    [Authorize]
    [HttpGet("MyProjects")]
    public async Task<ActionResult<List<ProjectResponseDto>>> GetUserProjectsAsync([FromQuery] string? search)
    {
        var userId = User.GetUserId();
        var response = await _projectService.GetUserProjectsAsync(userId, search);
        return Ok(response);
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectResponseDto>> GetProjectById(int id)
    {
        var userId = User.GetUserId();
        var response = await _projectService.GetProjectByIdAsync(id, userId);
        return Ok(response);
    }

    [Authorize]
    [HttpDelete("{projectId:int}")]
    public async Task<ActionResult> DeleteProjectAsync(int projectId)
    {
        var userId = User.GetUserId();
        await _projectService.DeleteProjectAsync(userId, projectId);
        return NoContent(); 
    }

    [Authorize]
    [HttpPut("{projectId:int}")]
    public async Task<ActionResult<ProjectResponseDto>> UpdateProjectAsync(int projectId, ProjectDto projectDto)
    {
        var userId = User.GetUserId();
        var response = await _projectService.UpdateProjectAsync(userId, projectId, projectDto);
        return Ok(response);
    }

    // --- Members ---

    [Authorize]
    [HttpGet("{projectId:int}/members")]
    public async Task<ActionResult<List<ProjectMemberDto>>> GetMembers(int projectId)
    {
        var userId = User.GetUserId();
        var response = await _projectService.GetMembersAsync(projectId, userId);
        return Ok(response);
    }

    [Authorize]
    [HttpPost("{projectId:int}/members")]
    public async Task<ActionResult<ProjectMemberDto>> AddMember(int projectId, AddMemberDto dto)
    {
        var ownerId = User.GetUserId();
        var response = await _projectService.AddMemberAsync(projectId, ownerId, dto);
        return Ok(response);
    }

    [Authorize]
    [HttpDelete("{projectId:int}/members/{targetUserId:int}")]
    public async Task<ActionResult> RemoveMember(int projectId, int targetUserId)
    {
        var ownerId = User.GetUserId();
        await _projectService.RemoveMemberAsync(projectId, ownerId, targetUserId);
        return NoContent();
    }
}