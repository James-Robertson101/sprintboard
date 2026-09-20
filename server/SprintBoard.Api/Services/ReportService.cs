namespace SprintBoard.Api.Services;
using SprintBoard.Api.DTOs;
using SprintBoard.Api.Models;
using SprintBoard.Api.Repositories;


public class ReportService : IReportService
{
    private readonly IReportRepository _reports;
    private readonly ISprintRepository _sprints;

    public ReportService(IReportRepository reports, ISprintRepository sprints)
    {
        _reports = reports;
        _sprints = sprints;
    }

    public async Task<SprintSummaryDto?> GetSprintSummaryAsync(int projectId, int sprintId)
    {
        var sprint = await _sprints.GetByIdAsync(projectId, sprintId);
        if (sprint is null) return null;

        var byStatus = await _reports.GetStatusCountsAsync(sprintId);
        var byPriority = await _reports.GetPriorityCountsAsync(sprintId);
        var byAssignee = await _reports.GetAssigneeWorkloadAsync(sprintId);

        var total = byStatus.Sum(s => s.Count);
        var done = byStatus.Where(s => s.Status == IssueStatus.Done).Sum(s => s.Count);

        return new SprintSummaryDto(
            sprint.Id, sprint.Name, sprint.StartDate, sprint.EndDate, sprint.Status,
            total, done,
            total == 0 ? 0 : Math.Round(done * 100.0 / total, 1),
            byStatus, byPriority, byAssignee);
    }

    public async Task<List<BurndownPointDto>?> GetBurndownAsync(int projectId, int sprintId)
    {
        var sprint = await _sprints.GetByIdAsync(projectId, sprintId);
        if (sprint is null) return null;

        var total = sprint.Issues.Count;
        var completed = await _reports.GetCompletionDatesAsync(sprintId);

        var start = sprint.StartDate.Date;
        var end = sprint.EndDate.Date;
        var last = sprint.Status == SprintStatus.Active
            ? DateTime.UtcNow.Date < end ? DateTime.UtcNow.Date : end
            : end;

        var totalDays = Math.Max((end - start).Days, 1);
        var points = new List<BurndownPointDto>();

        for (var day = start; day <= last; day = day.AddDays(1))
        {
            var doneByEndOfDay = completed.Count(c => c.Date <= day);
            var ideal = total * (1 - (double)(day - start).Days / totalDays);
            points.Add(new BurndownPointDto(day, total - doneByEndOfDay, Math.Round(ideal, 2)));
        }

        return points;
    }

    public Task<List<VelocityPointDto>> GetVelocityAsync(int projectId, int lastN = 6) =>
        _reports.GetVelocityAsync(projectId, lastN);
}