namespace SprintBoard.Api.Repositories;
using SprintBoard.Api.DTOs;

public interface IReportRepository
{
    Task<List<StatusCountDto>> GetStatusCountsAsync(int sprintId);
    Task<List<PriorityCountDto>> GetPriorityCountsAsync(int sprintId);
    Task<List<AssigneeWorkloadDto>> GetAssigneeWorkloadAsync(int sprintId);
    Task<List<DateTime>> GetCompletionDatesAsync(int sprintId);
    Task<List<VelocityPointDto>> GetVelocityAsync(int projectId, int lastN);
}