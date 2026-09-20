using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SprintBoard.Api.DTOs;
using SprintBoard.Api.Services;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:int}/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;
    public ReportsController(IReportService service) => _service = service;

    [HttpGet("sprints/{sprintId:int}/summary")]
    public async Task<IActionResult> Summary(int projectId, int sprintId)
    {
        // TODO: same project-membership check you use in your other controllers
        var result = await _service.GetSprintSummaryAsync(projectId, sprintId);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("sprints/{sprintId:int}/burndown")]
    public async Task<IActionResult> Burndown(int projectId, int sprintId)
    {
        var result = await _service.GetBurndownAsync(projectId, sprintId);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("velocity")]
    public async Task<IActionResult> Velocity(int projectId, [FromQuery] int sprints = 6) =>
        Ok(await _service.GetVelocityAsync(projectId, Math.Clamp(sprints, 1, 20)));
}