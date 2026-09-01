using SprintBoard.Api.DTOs;

namespace SprintBoard.Api.Services;

public interface IIssueService
{
    Task<List<IssueResponseDto>> GetIssuesForProjectAsync(int projectId, int userId);
    Task<IssueResponseDto> GetIssueByIdAsync(int projectId, int issueId, int userId);
    Task<IssueResponseDto> CreateIssueAsync(int projectId, int userId, CreateIssueDto dto);
    Task<IssueResponseDto> UpdateIssueAsync(int projectId, int issueId, int userId, UpdateIssueDto dto);
    Task DeleteIssueAsync(int projectId, int issueId, int userId);
}