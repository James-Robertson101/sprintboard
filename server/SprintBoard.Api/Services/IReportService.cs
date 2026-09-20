namespace SprintBoard.Api.Services;
using SprintBoard.Api.DTOs;
public interface IReportService
{
  Task<SprintSummaryDto?> GetSprintSummaryAsync(int projectId, int sprintId);
  Task<List<BurndownPointDto>?> GetBurndownAsync(int projectId, int sprintId);
  Task<List<VelocityPointDto>> GetVelocityAsync(int projectId, int lastN);
}