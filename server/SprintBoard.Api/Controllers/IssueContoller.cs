using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SprintBoard.Api.DTOs;
using SprintBoard.Api.Services;

namespace SprintBoard.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:int}/issues")]
public class IssuesController : ControllerBase
{
    private readonly IIssueService _issueService;

    public IssuesController(IIssueService issueService)
    {
        _issueService = issueService;
    }

    private int CurrentUserId =>
        User.GetUserId();

    [HttpGet]
    public async Task<ActionResult<List<IssueResponseDto>>> GetIssues(int projectId)
    {
        var issues = await _issueService.GetIssuesForProjectAsync(projectId, CurrentUserId);
        return Ok(issues);
    }

    [HttpGet("current")]
    public async Task<ActionResult<List<IssueResponseDto>>> GetCurrentIssues(int projectId)
    {
        var issues = await _issueService.GetCurrentIssues(projectId, CurrentUserId);
        return Ok(issues);
    }

    [HttpGet("{issueId:int}")]
    public async Task<ActionResult<IssueResponseDto>> GetIssueById(int projectId, int issueId)
    {
        var issue = await _issueService.GetIssueByIdAsync(projectId, issueId, CurrentUserId);
        return Ok(issue);
    }

    [HttpPost]
    public async Task<ActionResult<IssueResponseDto>> CreateIssue(int projectId, [FromBody] CreateIssueDto dto)
    {
        var created = await _issueService.CreateIssueAsync(projectId, CurrentUserId, dto);
        return CreatedAtAction(nameof(GetIssueById), new { projectId, issueId = created.Id }, created);
    }

    [HttpPut("{issueId:int}")]
    public async Task<ActionResult<IssueResponseDto>> UpdateIssue(int projectId, int issueId, [FromBody] UpdateIssueDto dto)
    {
        var updated = await _issueService.UpdateIssueAsync(projectId, issueId, CurrentUserId, dto);
        return Ok(updated);
    }

    [HttpDelete("{issueId:int}")]
    public async Task<IActionResult> DeleteIssue(int projectId, int issueId)
    {
        await _issueService.DeleteIssueAsync(projectId, issueId, CurrentUserId);
        return NoContent();
    }

    //Backlog methods:
    [HttpGet("~/api/projects/{projectId:int}/backlog")]
    public async Task<ActionResult<List<IssueResponseDto>>> GetBacklog(int projectId)
    {
        var issues = await _issueService.GetBacklogAsync(
        projectId,
        CurrentUserId);

        return Ok(issues);
    }
    
    [HttpPut("{issueId:int}/sprint")]
    public async Task<ActionResult<IssueResponseDto>> AssignIssueToSprint(int projectId, int issueId,
    [FromBody] AssignIssueToSprintDto dto)
    {
        var updated = await _issueService.AssignIssueToSprintAsync(
            projectId,
            issueId,
            CurrentUserId,
            dto);

        return Ok(updated);
    }
}