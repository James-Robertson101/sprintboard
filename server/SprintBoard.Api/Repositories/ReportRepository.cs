using Microsoft.EntityFrameworkCore;
using SprintBoard.Api.Data;
using SprintBoard.Api.DTOs;
using SprintBoard.Api.Models;

namespace SprintBoard.Api.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly AppDbContext _db;
    public ReportRepository(AppDbContext db) => _db = db;

    public Task<List<StatusCountDto>> GetStatusCountsAsync(int sprintId) =>
        _db.Issues
            .Where(i => i.SprintId == sprintId)
            .GroupBy(i => i.Status)
            .Select(g => new StatusCountDto(g.Key, g.Count()))
            .ToListAsync();

    public Task<List<PriorityCountDto>> GetPriorityCountsAsync(int sprintId) =>
        _db.Issues
            .Where(i => i.SprintId == sprintId)
            .GroupBy(i => i.Priority)
            .Select(g => new PriorityCountDto(g.Key, g.Count()))
            .ToListAsync();

    public Task<List<AssigneeWorkloadDto>> GetAssigneeWorkloadAsync(int sprintId) =>
        _db.Issues
            .Where(i => i.SprintId == sprintId)
            .GroupBy(i => new
            {
                i.AssigneeId,
                Name = i.Assignee != null ? i.Assignee.Name : "Unassigned",
                Avatar = i.Assignee != null ? i.Assignee.AvatarUrl : null
            })
            .Select(g => new AssigneeWorkloadDto(
                g.Key.AssigneeId,
                g.Key.Name,
                g.Key.Avatar,
                g.Count(),
                g.Count(i => i.Status == IssueStatus.Done)))
            .ToListAsync();

    public Task<List<DateTime>> GetCompletionDatesAsync(int sprintId) =>
        _db.Issues
            .Where(i => i.SprintId == sprintId && i.CompletedAt != null)
            .Select(i => i.CompletedAt!.Value)
            .ToListAsync();

    public async Task<List<VelocityPointDto>> GetVelocityAsync(int projectId, int lastN)
    {
        var rows = await _db.Sprints
            .Where(s => s.ProjectId == projectId && s.Status == SprintStatus.Completed)
            .OrderByDescending(s => s.EndDate)
            .Take(lastN)
            .Select(s => new VelocityPointDto(
                s.Id,
                s.Name,
                s.Issues.Count(),
                s.Issues.Count(i => i.Status == IssueStatus.Done)))
            .ToListAsync();

        rows.Reverse(); // oldest → newest for charting
        return rows;
    }
}