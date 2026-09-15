using Microsoft.EntityFrameworkCore;
using SprintBoard.Api.DTOs;
using SprintBoard.Api.Models;
using SprintBoard.Api.Repositories;
using SprintBoard.Api.Data;
namespace SprintBoard.Api.Services;

public class SprintService : ISprintService
{
    private readonly ISprintRepository _sprintRepository;
    private readonly AppDbContext _context;

    public SprintService(
        ISprintRepository sprintRepository,
        AppDbContext context)
    {
        _sprintRepository = sprintRepository;
        _context = context;
    }

    public async Task<List<SprintResponseDto>> GetSprintsAsync(
        int projectId,
        int currentUserId)
    {
        await EnsureProjectMemberAsync(projectId, currentUserId);

        var sprints = await _sprintRepository
            .GetByProjectIdAsync(projectId);

        return sprints.Select(MapToDto).ToList();
    }

    public async Task<SprintResponseDto> GetSprintAsync(
        int projectId,
        int sprintId,
        int currentUserId)
    {
        await EnsureProjectMemberAsync(projectId, currentUserId);

        var sprint = await GetSprintOrThrowAsync(
            projectId,
            sprintId);

        return MapToDto(sprint);
    }

    public async Task<SprintResponseDto> CreateSprintAsync(
        int projectId,
        int currentUserId,
        CreateSprintDto dto)
    {
        await EnsureProjectMemberAsync(projectId, currentUserId);

        ValidateDates(dto.StartDate, dto.EndDate);

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Sprint name is required.");

        var sprint = new Sprint
        {
            ProjectId = projectId,
            Name = dto.Name.Trim(),
            Goal = string.IsNullOrWhiteSpace(dto.Goal)
                ? null
                : dto.Goal.Trim(),
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = SprintStatus.Planned
        };

        await _sprintRepository.AddAsync(sprint);
        await _sprintRepository.SaveChangesAsync();

        return MapToDto(sprint);
    }

    public async Task<SprintResponseDto> UpdateSprintAsync(
        int projectId,
        int sprintId,
        int currentUserId,
        UpdateSprintDto dto)
    {
        await EnsureProjectMemberAsync(projectId, currentUserId);

        var sprint = await GetSprintOrThrowAsync(
            projectId,
            sprintId);

        if (sprint.Status == SprintStatus.Completed)
            throw new InvalidOperationException(
                "Completed sprints cannot be edited.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException(
                "Sprint name is required.");

        ValidateDates(dto.StartDate, dto.EndDate);

        sprint.Name = dto.Name.Trim();
        sprint.Goal = string.IsNullOrWhiteSpace(dto.Goal)
            ? null
            : dto.Goal.Trim();

        sprint.StartDate = dto.StartDate;
        sprint.EndDate = dto.EndDate;

        _sprintRepository.Update(sprint);

        await _sprintRepository.SaveChangesAsync();

        return MapToDto(sprint);
    }

    public async Task<SprintResponseDto> StartSprintAsync(
        int projectId,
        int sprintId,
        int currentUserId)
    {
        await EnsureProjectMemberAsync(projectId, currentUserId);

        var sprint = await GetSprintOrThrowAsync(
            projectId,
            sprintId);

        if (sprint.Status != SprintStatus.Planned)
            throw new InvalidOperationException(
                "Only planned sprints can be started.");

        if (await _sprintRepository.HasActiveSprintAsync(projectId))
            throw new InvalidOperationException(
                "This project already has an active sprint.");

        sprint.Status = SprintStatus.Active;

        _sprintRepository.Update(sprint);

        await _sprintRepository.SaveChangesAsync();

        return MapToDto(sprint);
    }

    public async Task<SprintResponseDto> CompleteSprintAsync(
        int projectId,
        int sprintId,
        int currentUserId)
    {
        await EnsureProjectMemberAsync(projectId, currentUserId);

        var sprint = await GetSprintOrThrowAsync(
            projectId,
            sprintId);

        if (sprint.Status != SprintStatus.Active)
            throw new InvalidOperationException(
                "Only active sprints can be completed.");

        sprint.Status = SprintStatus.Completed;

        _sprintRepository.Update(sprint);

        await _sprintRepository.SaveChangesAsync();

        return MapToDto(sprint);
    }

    public async Task DeleteSprintAsync(
        int projectId,
        int sprintId,
        int currentUserId)
    {
        await EnsureProjectMemberAsync(projectId, currentUserId);

        var sprint = await GetSprintOrThrowAsync(
            projectId,
            sprintId);

        if (sprint.Status != SprintStatus.Planned)
            throw new InvalidOperationException(
                "Only planned sprints can be deleted.");

        _sprintRepository.Delete(sprint);

        await _sprintRepository.SaveChangesAsync();
    }

    private async Task<Sprint> GetSprintOrThrowAsync(
        int projectId,
        int sprintId)
    {
        var sprint = await _sprintRepository
            .GetByIdAsync(projectId, sprintId);

        if (sprint == null)
            throw new KeyNotFoundException(
                "Sprint not found.");

        return sprint;
    }

    private static void ValidateDates(
        DateTime startDate,
        DateTime endDate)
    {
        if (endDate <= startDate)
        {
            throw new ArgumentException(
                "Sprint end date must be after the start date.");
        }
    }

    private static SprintResponseDto MapToDto(Sprint sprint)
    {
        return new SprintResponseDto(
            sprint.Id,
            sprint.ProjectId,
            sprint.Name,
            sprint.Goal,
            sprint.StartDate,
            sprint.EndDate,
            sprint.Status,
            sprint.Issues.Count
        );
    }

    private async Task EnsureProjectMemberAsync(
        int projectId,
        int userId)
    {
        var isMember = await _context.ProjectMembers
            .AnyAsync(pm =>
                pm.ProjectId == projectId &&
                pm.UserId == userId);

        if (!isMember)
        {
            throw new UnauthorizedAccessException(
                "You are not a member of this project.");
        }
    }
}