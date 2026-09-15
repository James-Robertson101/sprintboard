using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SprintBoard.Api.DTOs;
using SprintBoard.Api.Services;

namespace SprintBoard.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:int}/sprints")]
public class SprintsController : ControllerBase
{
    private readonly ISprintService _sprintService;

    public SprintsController(ISprintService sprintService)
    {
        _sprintService = sprintService;
    }

    private int CurrentUserId => User.GetUserId();

    [HttpGet]
    public async Task<ActionResult<List<SprintResponseDto>>> GetSprints(
        int projectId)
    {
        var sprints = await _sprintService.GetSprintsAsync(
            projectId,
            CurrentUserId);

        return Ok(sprints);
    }

    [HttpGet("{sprintId:int}")]
    public async Task<ActionResult<SprintResponseDto>> GetSprint(
        int projectId,
        int sprintId)
    {
        var sprint = await _sprintService.GetSprintAsync(
            projectId,
            sprintId,
            CurrentUserId);

        return Ok(sprint);
    }

    [HttpPost]
    public async Task<ActionResult<SprintResponseDto>> CreateSprint(
        int projectId,
        [FromBody] CreateSprintDto dto)
    {
        var sprint = await _sprintService.CreateSprintAsync(
            projectId,
            CurrentUserId,
            dto);

        return CreatedAtAction(
            nameof(GetSprint),
            new
            {
                projectId,
                sprintId = sprint.Id
            },
            sprint);
    }

    [HttpPut("{sprintId:int}")]
    public async Task<ActionResult<SprintResponseDto>> UpdateSprint(
        int projectId,
        int sprintId,
        [FromBody] UpdateSprintDto dto)
    {
        var sprint = await _sprintService.UpdateSprintAsync(
            projectId,
            sprintId,
            CurrentUserId,
            dto);

        return Ok(sprint);
    }

    [HttpPost("{sprintId:int}/start")]
    public async Task<ActionResult<SprintResponseDto>> StartSprint(
        int projectId,
        int sprintId)
    {
        var sprint = await _sprintService.StartSprintAsync(
            projectId,
            sprintId,
            CurrentUserId);

        return Ok(sprint);
    }

    [HttpPost("{sprintId:int}/complete")]
    public async Task<ActionResult<SprintResponseDto>> CompleteSprint(
        int projectId,
        int sprintId)
    {
        var sprint = await _sprintService.CompleteSprintAsync(
            projectId,
            sprintId,
            CurrentUserId);

        return Ok(sprint);
    }

    [HttpDelete("{sprintId:int}")]
    public async Task<IActionResult> DeleteSprint(
        int projectId,
        int sprintId)
    {
        await _sprintService.DeleteSprintAsync(
            projectId,
            sprintId,
            CurrentUserId);

        return NoContent();
    }
}