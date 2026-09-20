using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SprintBoard.Api.DTOs;
using SprintBoard.Api.Exceptions;
using SprintBoard.Api.Hubs;
using SprintBoard.Api.Models;
using SprintBoard.Api.Repositories;
using SprintBoard.Api.Services;
using Xunit;

namespace SprintBoard.Api.Tests.Services;

public class IssueServiceTests
{
    private readonly Mock<IIssueRepository> _issueRepositoryMock;
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<ISprintRepository> _sprintRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;

    // SignalR mocks: IHubContext -> IHubClients -> IClientProxy
    private readonly Mock<IHubContext<SprintBoardHub>> _hubContextMock;
    private readonly Mock<IHubClients> _hubClientsMock;
    private readonly Mock<IClientProxy> _clientProxyMock;

    private readonly IssueService _sut; // system under test

    public IssueServiceTests()
    {
        _issueRepositoryMock = new Mock<IIssueRepository>();
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _sprintRepositoryMock = new Mock<ISprintRepository>();

        _hubContextMock = new Mock<IHubContext<SprintBoardHub>>();
        _hubClientsMock = new Mock<IHubClients>();
        _clientProxyMock = new Mock<IClientProxy>();

        _hubContextMock.Setup(h => h.Clients).Returns(_hubClientsMock.Object);
        _hubClientsMock
            .Setup(c => c.Group(It.IsAny<string>()))
            .Returns(_clientProxyMock.Object);

        // SendAsync(...) is an extension method that ends up calling SendCoreAsync,
        // so SendCoreAsync is the method we set up and verify.
        _clientProxyMock
            .Setup(p => p.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new IssueService(
            _issueRepositoryMock.Object,
            _projectRepositoryMock.Object,
            _userRepositoryMock.Object,
            _sprintRepositoryMock.Object,
            _hubContextMock.Object,
            NullLogger<IssueService>.Instance);
    }

    // --- Helpers ---

    private static Project CreateProject(int projectId, params int[] memberUserIds)
    {
        return new Project
        {
            Id = projectId,
            ProjectMembers = memberUserIds
                .Select(id => new ProjectMember { UserId = id })
                .ToList()
        };
    }

    private static User CreateUser(int id, string name = "Test User", string? avatarUrl = null)
    {
        return new User
        {
            Id = id,
            Name = name,
            AvatarUrl = avatarUrl
        };
    }

    private static Issue CreateIssue(
        int id,
        int projectId,
        string name = "Fix login bug",
        IssueStatus status = IssueStatus.Todo,
        User? assignee = null)
    {
        return new Issue
        {
            Id = id,
            ProjectId = projectId,
            Name = name,
            Description = "Some description",
            Priority = Priority.Medium,
            Status = status,
            AssigneeId = assignee?.Id,
            Assignee = assignee,
            CreatedById = 1,
            CreatedAt = DateTime.UtcNow
        };
    }

    private void VerifyBroadcast(int projectId, string method, Times? times = null)
    {
        _hubClientsMock.Verify(c => c.Group($"project-{projectId}"), times ?? Times.Once());
        _clientProxyMock.Verify(
            p => p.SendCoreAsync(method, It.IsAny<object?[]>(), It.IsAny<CancellationToken>()),
            times ?? Times.Once());
    }

    private void VerifyNothingBroadcast()
    {
        _clientProxyMock.Verify(
            p => p.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()),
            Times.Never());
    }

    // --- GetIssuesForProjectAsync ---

    [Fact]
    public async Task GetIssuesForProjectAsync_WhenUserIsMember_ReturnsMappedIssues()
    {
        // Arrange
        const int projectId = 1;
        const int userId = 10;
        var project = CreateProject(projectId, userId);
        var issues = new List<Issue>
        {
            CreateIssue(1, projectId, "Issue A"),
            CreateIssue(2, projectId, "Issue B")
        };

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock.Setup(r => r.GetByProjectIdAsync(projectId)).ReturnsAsync(issues);

        // Act
        var result = await _sut.GetIssuesForProjectAsync(projectId, userId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Issue A", result[0].Name);
        Assert.Equal("Issue B", result[1].Name);
    }

    [Fact]
    public async Task GetIssuesForProjectAsync_WhenProjectDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync((Project?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.GetIssuesForProjectAsync(1, 10));

        _issueRepositoryMock.Verify(r => r.GetByProjectIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetIssuesForProjectAsync_WhenUserIsNotMember_ThrowsNotFoundException()
    {
        // Arrange
        const int projectId = 1;
        var project = CreateProject(projectId, 999); // userId 10 is not a member

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.GetIssuesForProjectAsync(projectId, 10));

        _issueRepositoryMock.Verify(r => r.GetByProjectIdAsync(It.IsAny<int>()), Times.Never);
    }

    // --- GetIssueByIdAsync ---

    [Fact]
    public async Task GetIssueByIdAsync_WhenIssueExistsInProject_ReturnsMappedDto()
    {
        // Arrange
        const int projectId = 1;
        const int issueId = 5;
        const int userId = 10;
        var project = CreateProject(projectId, userId);
        var assignee = CreateUser(20, "Ada Lovelace", "https://example.com/ada.png");
        var issue = CreateIssue(issueId, projectId, "Fix crash", assignee: assignee);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock.Setup(r => r.GetByIdAsync(issueId)).ReturnsAsync(issue);

        // Act
        var result = await _sut.GetIssueByIdAsync(projectId, issueId, userId);

        // Assert
        Assert.Equal(issueId, result.Id);
        Assert.Equal("Fix crash", result.Name);
        Assert.NotNull(result.Assignee);
        Assert.Equal(assignee.Id, result.Assignee!.Id);
        Assert.Equal(assignee.Name, result.Assignee.Name);
        Assert.Equal(assignee.AvatarUrl, result.Assignee.AvatarUrl);
    }

    [Fact]
    public async Task GetIssueByIdAsync_WhenIssueHasNoAssignee_ReturnsDtoWithNullAssignee()
    {
        // Arrange
        const int projectId = 1;
        const int issueId = 5;
        const int userId = 10;
        var project = CreateProject(projectId, userId);
        var issue = CreateIssue(issueId, projectId);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock.Setup(r => r.GetByIdAsync(issueId)).ReturnsAsync(issue);

        // Act
        var result = await _sut.GetIssueByIdAsync(projectId, issueId, userId);

        // Assert
        Assert.Null(result.Assignee);
    }

    [Fact]
    public async Task GetIssueByIdAsync_WhenIssueDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        const int projectId = 1;
        const int userId = 10;
        var project = CreateProject(projectId, userId);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Issue?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.GetIssueByIdAsync(projectId, 999, userId));
    }

    [Fact]
    public async Task GetIssueByIdAsync_WhenIssueBelongsToDifferentProject_ThrowsNotFoundException()
    {
        // Arrange
        const int projectId = 1;
        const int userId = 10;
        var project = CreateProject(projectId, userId);
        var issue = CreateIssue(5, projectId: 999); // belongs to a different project

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(issue);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.GetIssueByIdAsync(projectId, 5, userId));
    }

    [Fact]
    public async Task GetIssueByIdAsync_WhenUserIsNotProjectMember_ThrowsNotFoundException()
    {
        // Arrange
        const int projectId = 1;
        var project = CreateProject(projectId, 999);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.GetIssueByIdAsync(projectId, 5, 10));

        _issueRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    // --- CreateIssueAsync ---

    [Fact]
    public async Task CreateIssueAsync_WithNoAssignee_CreatesIssueWithGivenStatus()
    {
        // Arrange
        const int projectId = 1;
        const int userId = 10;
        var project = CreateProject(projectId, userId);
        var dto = new CreateIssueDto("New feature", "Description here", Priority.High, null, IssueStatus.Todo);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Issue>()))
            .ReturnsAsync((Issue i) => i);

        // Act
        var result = await _sut.CreateIssueAsync(projectId, userId, dto);

        // Assert
        Assert.Equal("New feature", result.Name);
        Assert.Equal(IssueStatus.Todo, result.Status);
        Assert.Null(result.Assignee);

        _issueRepositoryMock.Verify(r => r.CreateAsync(It.Is<Issue>(i =>
            i.ProjectId == projectId &&
            i.Name == dto.Name &&
            i.Description == dto.Description &&
            i.Priority == dto.Priority &&
            i.Status == IssueStatus.Todo &&
            i.AssigneeId == null &&
            i.CreatedById == userId)), Times.Once);

        _userRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CreateIssueAsync_WithValidAssignee_CreatesIssueWithAssignee()
    {
        // Arrange
        const int projectId = 1;
        const int userId = 10;
        const int assigneeId = 20;
        var project = CreateProject(projectId, userId, assigneeId);
        var assignee = CreateUser(assigneeId, "Grace Hopper");
        var dto = new CreateIssueDto("New feature", "Description here", Priority.Low, assigneeId, IssueStatus.Todo);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(assigneeId)).ReturnsAsync(assignee);
        _issueRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Issue>()))
            .ReturnsAsync((Issue i) => i);

        // Act
        var result = await _sut.CreateIssueAsync(projectId, userId, dto);

        // Assert
        Assert.NotNull(result.Assignee);
        Assert.Equal(assigneeId, result.Assignee!.Id);
        Assert.Equal("Grace Hopper", result.Assignee.Name);

        _issueRepositoryMock.Verify(r => r.CreateAsync(It.Is<Issue>(i =>
            i.AssigneeId == assigneeId)), Times.Once);
    }

    [Fact]
    public async Task CreateIssueAsync_WhenActiveSprintExists_AssignsIssueToActiveSprint()
    {
        // Arrange
        const int projectId = 1;
        const int userId = 10;
        const int sprintId = 7;
        var project = CreateProject(projectId, userId);
        var dto = new CreateIssueDto("New feature", "Description", Priority.Medium, null, IssueStatus.Todo);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _sprintRepositoryMock
            .Setup(r => r.GetActiveSprintAsync(projectId))
            .ReturnsAsync(new Sprint { Id = sprintId, ProjectId = projectId });
        _issueRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Issue>()))
            .ReturnsAsync((Issue i) => i);

        // Act
        await _sut.CreateIssueAsync(projectId, userId, dto);

        // Assert
        _issueRepositoryMock.Verify(r => r.CreateAsync(It.Is<Issue>(i =>
            i.SprintId == sprintId)), Times.Once);
    }

    [Fact]
    public async Task CreateIssueAsync_WhenCreatedAsTodo_LeavesCompletedAtNull()
    {
        // Arrange
        const int projectId = 1;
        const int userId = 10;
        var project = CreateProject(projectId, userId);
        var dto = new CreateIssueDto("New feature", "Description", Priority.Medium, null, IssueStatus.Todo);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Issue>()))
            .ReturnsAsync((Issue i) => i);

        // Act
        await _sut.CreateIssueAsync(projectId, userId, dto);

        // Assert
        _issueRepositoryMock.Verify(r => r.CreateAsync(It.Is<Issue>(i =>
            i.CompletedAt == null)), Times.Once);
    }

    [Fact]
    public async Task CreateIssueAsync_WhenCreatedAsDone_SetsCompletedAt()
    {
        // Arrange
        const int projectId = 1;
        const int userId = 10;
        var project = CreateProject(projectId, userId);
        var dto = new CreateIssueDto("Already done", "Description", Priority.Medium, null, IssueStatus.Done);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Issue>()))
            .ReturnsAsync((Issue i) => i);

        // Act
        await _sut.CreateIssueAsync(projectId, userId, dto);

        // Assert
        _issueRepositoryMock.Verify(r => r.CreateAsync(It.Is<Issue>(i =>
            i.Status == IssueStatus.Done &&
            i.CompletedAt != null)), Times.Once);
    }

    [Fact]
    public async Task CreateIssueAsync_OnSuccess_BroadcastsIssueCreatedToProjectGroup()
    {
        // Arrange
        const int projectId = 1;
        const int userId = 10;
        var project = CreateProject(projectId, userId);
        var dto = new CreateIssueDto("New feature", "Description", Priority.Medium, null, IssueStatus.Todo);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Issue>()))
            .ReturnsAsync((Issue i) => i);

        // Act
        await _sut.CreateIssueAsync(projectId, userId, dto);

        // Assert
        _hubClientsMock.Verify(c => c.Group($"project-{projectId}"), Times.Once);
        _clientProxyMock.Verify(p => p.SendCoreAsync(
            "IssueCreated",
            It.Is<object?[]>(args => args.Length == 1 && args[0] is IssueResponseDto),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateIssueAsync_WhenAssigneeDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        const int projectId = 1;
        const int userId = 10;
        var project = CreateProject(projectId, userId);
        var dto = new CreateIssueDto("New feature", "Description", Priority.Medium, 999, IssueStatus.Todo);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.CreateIssueAsync(projectId, userId, dto));

        _issueRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Issue>()), Times.Never);
        VerifyNothingBroadcast();
    }

    [Fact]
    public async Task CreateIssueAsync_WhenAssigneeIsNotProjectMember_ThrowsForbiddenException()
    {
        // Arrange
        const int projectId = 1;
        const int userId = 10;
        const int assigneeId = 20;
        var project = CreateProject(projectId, userId); // assignee not included
        var assignee = CreateUser(assigneeId, "Outsider");
        var dto = new CreateIssueDto("New feature", "Description", Priority.Medium, assigneeId, IssueStatus.Todo);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(assigneeId)).ReturnsAsync(assignee);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => _sut.CreateIssueAsync(projectId, userId, dto));

        _issueRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Issue>()), Times.Never);
        VerifyNothingBroadcast();
    }

    [Fact]
    public async Task CreateIssueAsync_WhenCreatorIsNotProjectMember_ThrowsNotFoundException()
    {
        // Arrange
        const int projectId = 1;
        var project = CreateProject(projectId, 999);
        var dto = new CreateIssueDto("New feature", "Description", Priority.Medium, null, IssueStatus.Todo);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.CreateIssueAsync(projectId, 10, dto));

        _issueRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Issue>()), Times.Never);
        VerifyNothingBroadcast();
    }

    // --- UpdateIssueAsync ---

    [Fact]
    public async Task UpdateIssueAsync_WhenIssueExists_UpdatesFieldsAndReturnsDto()
    {
        // Arrange
        const int projectId = 1;
        const int issueId = 5;
        const int userId = 10;
        var project = CreateProject(projectId, userId);
        var existingIssue = CreateIssue(issueId, projectId, "Old name", IssueStatus.Todo);
        var dto = new UpdateIssueDto("Updated name", "Updated description", Priority.High, IssueStatus.InProgress, null);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock.Setup(r => r.GetByIdAsync(issueId)).ReturnsAsync(existingIssue);
        _issueRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Issue>()))
            .ReturnsAsync((Issue i) => i);

        // Act
        var result = await _sut.UpdateIssueAsync(projectId, issueId, userId, dto);

        // Assert
        Assert.Equal("Updated name", result.Name);
        Assert.Equal("Updated description", result.Description);
        Assert.Equal(Priority.High, result.Priority);
        Assert.Equal(IssueStatus.InProgress, result.Status);
        Assert.NotNull(result.UpdatedAt);

        _issueRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Issue>(i =>
            i.Name == "Updated name" &&
            i.Status == IssueStatus.InProgress &&
            i.AssigneeId == null)), Times.Once);
    }

    [Fact]
    public async Task UpdateIssueAsync_WithNewAssignee_UpdatesAssignee()
    {
        // Arrange
        const int projectId = 1;
        const int issueId = 5;
        const int userId = 10;
        const int assigneeId = 20;
        var project = CreateProject(projectId, userId, assigneeId);
        var existingIssue = CreateIssue(issueId, projectId);
        var assignee = CreateUser(assigneeId, "Linus Torvalds");
        var dto = new UpdateIssueDto("Name", "Description", Priority.Medium, IssueStatus.Todo, assigneeId);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock.Setup(r => r.GetByIdAsync(issueId)).ReturnsAsync(existingIssue);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(assigneeId)).ReturnsAsync(assignee);
        _issueRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Issue>()))
            .ReturnsAsync((Issue i) => i);

        // Act
        var result = await _sut.UpdateIssueAsync(projectId, issueId, userId, dto);

        // Assert
        Assert.NotNull(result.Assignee);
        Assert.Equal(assigneeId, result.Assignee!.Id);
    }

    [Fact]
    public async Task UpdateIssueAsync_WhenUnassigningExistingAssignee_ClearsAssignee()
    {
        // Arrange
        const int projectId = 1;
        const int issueId = 5;
        const int userId = 10;
        var project = CreateProject(projectId, userId);
        var currentAssignee = CreateUser(20, "Previous Assignee");
        var existingIssue = CreateIssue(issueId, projectId, assignee: currentAssignee);
        var dto = new UpdateIssueDto("Name", "Description", Priority.Medium, IssueStatus.Todo, null);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock.Setup(r => r.GetByIdAsync(issueId)).ReturnsAsync(existingIssue);
        _issueRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Issue>()))
            .ReturnsAsync((Issue i) => i);

        // Act
        var result = await _sut.UpdateIssueAsync(projectId, issueId, userId, dto);

        // Assert
        Assert.Null(result.Assignee);
    }

    [Fact]
    public async Task UpdateIssueAsync_WhenMovedToDone_SetsCompletedAt()
    {
        // Arrange
        const int projectId = 1;
        const int issueId = 5;
        const int userId = 10;
        var project = CreateProject(projectId, userId);
        var existingIssue = CreateIssue(issueId, projectId, status: IssueStatus.InProgress);
        var dto = new UpdateIssueDto("Name", "Description", Priority.Medium, IssueStatus.Done, null);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock.Setup(r => r.GetByIdAsync(issueId)).ReturnsAsync(existingIssue);
        _issueRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Issue>()))
            .ReturnsAsync((Issue i) => i);

        var before = DateTime.UtcNow;

        // Act
        await _sut.UpdateIssueAsync(projectId, issueId, userId, dto);

        var after = DateTime.UtcNow;

        // Assert
        Assert.NotNull(existingIssue.CompletedAt);
        Assert.InRange(existingIssue.CompletedAt!.Value, before, after);
    }

    [Fact]
    public async Task UpdateIssueAsync_WhenReopenedFromDone_ClearsCompletedAt()
    {
        // Arrange
        const int projectId = 1;
        const int issueId = 5;
        const int userId = 10;
        var project = CreateProject(projectId, userId);
        var existingIssue = CreateIssue(issueId, projectId, status: IssueStatus.Done);
        existingIssue.CompletedAt = DateTime.UtcNow.AddDays(-1);
        var dto = new UpdateIssueDto("Name", "Description", Priority.Medium, IssueStatus.InProgress, null);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock.Setup(r => r.GetByIdAsync(issueId)).ReturnsAsync(existingIssue);
        _issueRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Issue>()))
            .ReturnsAsync((Issue i) => i);

        // Act
        await _sut.UpdateIssueAsync(projectId, issueId, userId, dto);

        // Assert
        Assert.Null(existingIssue.CompletedAt);
    }

    [Fact]
    public async Task UpdateIssueAsync_WhenAlreadyDoneAndStaysDone_KeepsOriginalCompletedAt()
    {
        // Arrange
        const int projectId = 1;
        const int issueId = 5;
        const int userId = 10;
        var project = CreateProject(projectId, userId);
        var originalCompletedAt = DateTime.UtcNow.AddDays(-3);
        var existingIssue = CreateIssue(issueId, projectId, status: IssueStatus.Done);
        existingIssue.CompletedAt = originalCompletedAt;
        // Only the name changes; status stays Done.
        var dto = new UpdateIssueDto("Renamed", "Description", Priority.Medium, IssueStatus.Done, null);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock.Setup(r => r.GetByIdAsync(issueId)).ReturnsAsync(existingIssue);
        _issueRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Issue>()))
            .ReturnsAsync((Issue i) => i);

        // Act
        await _sut.UpdateIssueAsync(projectId, issueId, userId, dto);

        // Assert
        Assert.Equal(originalCompletedAt, existingIssue.CompletedAt);
    }

    [Fact]
    public async Task UpdateIssueAsync_WhenStatusChangesBetweenNonDoneStates_LeavesCompletedAtNull()
    {
        // Arrange
        const int projectId = 1;
        const int issueId = 5;
        const int userId = 10;
        var project = CreateProject(projectId, userId);
        var existingIssue = CreateIssue(issueId, projectId, status: IssueStatus.Todo);
        var dto = new UpdateIssueDto("Name", "Description", Priority.Medium, IssueStatus.InReview, null);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock.Setup(r => r.GetByIdAsync(issueId)).ReturnsAsync(existingIssue);
        _issueRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Issue>()))
            .ReturnsAsync((Issue i) => i);

        // Act
        await _sut.UpdateIssueAsync(projectId, issueId, userId, dto);

        // Assert
        Assert.Null(existingIssue.CompletedAt);
    }

    [Fact]
    public async Task UpdateIssueAsync_OnSuccess_BroadcastsIssueUpdatedToProjectGroup()
    {
        // Arrange
        const int projectId = 1;
        const int issueId = 5;
        const int userId = 10;
        var project = CreateProject(projectId, userId);
        var existingIssue = CreateIssue(issueId, projectId);
        var dto = new UpdateIssueDto("Name", "Description", Priority.Medium, IssueStatus.Todo, null);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock.Setup(r => r.GetByIdAsync(issueId)).ReturnsAsync(existingIssue);
        _issueRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Issue>()))
            .ReturnsAsync((Issue i) => i);

        // Act
        await _sut.UpdateIssueAsync(projectId, issueId, userId, dto);

        // Assert
        _hubClientsMock.Verify(c => c.Group($"project-{projectId}"), Times.Once);
        _clientProxyMock.Verify(p => p.SendCoreAsync(
            "IssueUpdated",
            It.Is<object?[]>(args => args.Length == 1 && args[0] is IssueResponseDto),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateIssueAsync_WhenIssueDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        const int projectId = 1;
        const int userId = 10;
        var project = CreateProject(projectId, userId);
        var dto = new UpdateIssueDto("Name", "Description", Priority.Medium, IssueStatus.Todo, null);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Issue?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.UpdateIssueAsync(projectId, 999, userId, dto));

        _issueRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Issue>()), Times.Never);
        VerifyNothingBroadcast();
    }

    [Fact]
    public async Task UpdateIssueAsync_WhenIssueBelongsToDifferentProject_ThrowsNotFoundException()
    {
        // Arrange
        const int projectId = 1;
        const int userId = 10;
        var project = CreateProject(projectId, userId);
        var issue = CreateIssue(5, projectId: 999);
        var dto = new UpdateIssueDto("Name", "Description", Priority.Medium, IssueStatus.Todo, null);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(issue);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.UpdateIssueAsync(projectId, 5, userId, dto));

        _issueRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Issue>()), Times.Never);
        VerifyNothingBroadcast();
    }

    [Fact]
    public async Task UpdateIssueAsync_WhenNewAssigneeIsNotProjectMember_ThrowsForbiddenException()
    {
        // Arrange
        const int projectId = 1;
        const int issueId = 5;
        const int userId = 10;
        const int assigneeId = 20;
        var project = CreateProject(projectId, userId); // assignee not a member
        var existingIssue = CreateIssue(issueId, projectId);
        var assignee = CreateUser(assigneeId, "Outsider");
        var dto = new UpdateIssueDto("Name", "Description", Priority.Medium, IssueStatus.Todo, assigneeId);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock.Setup(r => r.GetByIdAsync(issueId)).ReturnsAsync(existingIssue);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(assigneeId)).ReturnsAsync(assignee);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => _sut.UpdateIssueAsync(projectId, issueId, userId, dto));

        _issueRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Issue>()), Times.Never);
        VerifyNothingBroadcast();
    }

    // --- DeleteIssueAsync ---

    [Fact]
    public async Task DeleteIssueAsync_WhenIssueExists_CallsRepositoryDelete()
    {
        // Arrange
        const int projectId = 1;
        const int issueId = 5;
        const int userId = 10;
        var project = CreateProject(projectId, userId);
        var issue = CreateIssue(issueId, projectId);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock.Setup(r => r.GetByIdAsync(issueId)).ReturnsAsync(issue);

        // Act
        await _sut.DeleteIssueAsync(projectId, issueId, userId);

        // Assert
        _issueRepositoryMock.Verify(r => r.DeleteAsync(issue), Times.Once);
    }

    [Fact]
    public async Task DeleteIssueAsync_OnSuccess_BroadcastsIssueDeletedWithIssueId()
    {
        // Arrange
        const int projectId = 1;
        const int issueId = 5;
        const int userId = 10;
        var project = CreateProject(projectId, userId);
        var issue = CreateIssue(issueId, projectId);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock.Setup(r => r.GetByIdAsync(issueId)).ReturnsAsync(issue);

        // Act
        await _sut.DeleteIssueAsync(projectId, issueId, userId);

        // Assert
        _hubClientsMock.Verify(c => c.Group($"project-{projectId}"), Times.Once);
        _clientProxyMock.Verify(p => p.SendCoreAsync(
            "IssueDeleted",
            It.Is<object?[]>(args => args.Length == 1 && args[0] is int && (int)args[0]! == issueId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteIssueAsync_WhenIssueDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        const int projectId = 1;
        const int userId = 10;
        var project = CreateProject(projectId, userId);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Issue?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.DeleteIssueAsync(projectId, 999, userId));

        _issueRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Issue>()), Times.Never);
        VerifyNothingBroadcast();
    }

    [Fact]
    public async Task DeleteIssueAsync_WhenIssueBelongsToDifferentProject_ThrowsNotFoundException()
    {
        // Arrange
        const int projectId = 1;
        const int userId = 10;
        var project = CreateProject(projectId, userId);
        var issue = CreateIssue(5, projectId: 999);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(issue);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.DeleteIssueAsync(projectId, 5, userId));

        _issueRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Issue>()), Times.Never);
        VerifyNothingBroadcast();
    }

    [Fact]
    public async Task DeleteIssueAsync_WhenUserIsNotProjectMember_ThrowsNotFoundException()
    {
        // Arrange
        const int projectId = 1;
        var project = CreateProject(projectId, 999);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.DeleteIssueAsync(projectId, 5, 10));

        _issueRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        _issueRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Issue>()), Times.Never);
    }

    // --- AssignIssueToSprintAsync ---

    [Fact]
    public async Task AssignIssueToSprintAsync_WithValidSprint_AssignsIssueAndBroadcasts()
    {
        // Arrange
        const int projectId = 1;
        const int issueId = 5;
        const int userId = 10;
        const int sprintId = 3;
        var project = CreateProject(projectId, userId);
        var issue = CreateIssue(issueId, projectId);
        var sprint = new Sprint { Id = sprintId, ProjectId = projectId };

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock.Setup(r => r.GetByIdAsync(issueId)).ReturnsAsync(issue);
        _sprintRepositoryMock.Setup(r => r.GetByIdAsync(projectId, sprintId)).ReturnsAsync(sprint);
        _issueRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Issue>()))
            .ReturnsAsync((Issue i) => i);

        // Act
        await _sut.AssignIssueToSprintAsync(projectId, issueId, userId, new AssignIssueToSprintDto(sprintId));

        // Assert
        Assert.Equal(sprintId, issue.SprintId);
        Assert.NotNull(issue.UpdatedAt);
        _issueRepositoryMock.Verify(r => r.UpdateAsync(issue), Times.Once);
        VerifyBroadcast(projectId, "IssueUpdated");
    }

    [Fact]
    public async Task AssignIssueToSprintAsync_WithNullSprintId_MovesIssueToBacklog()
    {
        // Arrange
        const int projectId = 1;
        const int issueId = 5;
        const int userId = 10;
        var project = CreateProject(projectId, userId);
        var issue = CreateIssue(issueId, projectId);
        issue.SprintId = 3;

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock.Setup(r => r.GetByIdAsync(issueId)).ReturnsAsync(issue);
        _issueRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Issue>()))
            .ReturnsAsync((Issue i) => i);

        // Act
        await _sut.AssignIssueToSprintAsync(projectId, issueId, userId, new AssignIssueToSprintDto(null));

        // Assert
        Assert.Null(issue.SprintId);
        _sprintRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task AssignIssueToSprintAsync_WhenIssueIsDone_DoesNotChangeCompletedAt()
    {
        // Arrange
        const int projectId = 1;
        const int issueId = 5;
        const int userId = 10;
        const int sprintId = 3;
        var project = CreateProject(projectId, userId);
        var originalCompletedAt = DateTime.UtcNow.AddDays(-2);
        var issue = CreateIssue(issueId, projectId, status: IssueStatus.Done);
        issue.CompletedAt = originalCompletedAt;
        var sprint = new Sprint { Id = sprintId, ProjectId = projectId };

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock.Setup(r => r.GetByIdAsync(issueId)).ReturnsAsync(issue);
        _sprintRepositoryMock.Setup(r => r.GetByIdAsync(projectId, sprintId)).ReturnsAsync(sprint);
        _issueRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Issue>()))
            .ReturnsAsync((Issue i) => i);

        // Act
        await _sut.AssignIssueToSprintAsync(projectId, issueId, userId, new AssignIssueToSprintDto(sprintId));

        // Assert
        Assert.Equal(originalCompletedAt, issue.CompletedAt);
    }

    [Fact]
    public async Task AssignIssueToSprintAsync_WhenSprintDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        const int projectId = 1;
        const int issueId = 5;
        const int userId = 10;
        var project = CreateProject(projectId, userId);
        var issue = CreateIssue(issueId, projectId);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock.Setup(r => r.GetByIdAsync(issueId)).ReturnsAsync(issue);
        _sprintRepositoryMock
            .Setup(r => r.GetByIdAsync(projectId, 999))
            .ReturnsAsync((Sprint?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.AssignIssueToSprintAsync(projectId, issueId, userId, new AssignIssueToSprintDto(999)));

        _issueRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Issue>()), Times.Never);
        VerifyNothingBroadcast();
    }

    [Fact]
    public async Task AssignIssueToSprintAsync_WhenIssueDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        const int projectId = 1;
        const int userId = 10;
        var project = CreateProject(projectId, userId);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Issue?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.AssignIssueToSprintAsync(projectId, 999, userId, new AssignIssueToSprintDto(1)));

        _issueRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Issue>()), Times.Never);
        VerifyNothingBroadcast();
    }

    [Fact]
    public async Task AssignIssueToSprintAsync_WhenIssueBelongsToDifferentProject_ThrowsNotFoundException()
    {
        // Arrange
        const int projectId = 1;
        const int userId = 10;
        var project = CreateProject(projectId, userId);
        var issue = CreateIssue(5, projectId: 999);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _issueRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(issue);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.AssignIssueToSprintAsync(projectId, 5, userId, new AssignIssueToSprintDto(1)));

        _issueRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Issue>()), Times.Never);
        VerifyNothingBroadcast();
    }
}