using Moq;
using SprintBoard.Api.DTOs;
using SprintBoard.Api.Exceptions;
using SprintBoard.Api.Models;
using SprintBoard.Api.Repositories;
using SprintBoard.Api.Services;
using Xunit;

namespace SprintBoard.Api.Tests.Services;

public class IssueServiceTests
{
    private readonly Mock<IIssueRepository> _issueRepositoryMock;
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly IssueService _sut; // system under test

    public IssueServiceTests()
    {
        _issueRepositoryMock = new Mock<IIssueRepository>();
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _sut = new IssueService(
            _issueRepositoryMock.Object,
            _projectRepositoryMock.Object,
            _userRepositoryMock.Object);
    }


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
    public async Task CreateIssueAsync_WithNoAssignee_CreatesIssueWithTodoStatus()
    {
        // Arrange
        const int projectId = 1;
        const int userId = 10;
        var project = CreateProject(projectId, userId);
        var dto = new CreateIssueDto("New feature", "Description here", Priority.High, null);

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
        var dto = new CreateIssueDto("New feature", "Description here", Priority.Low, assigneeId);

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
    public async Task CreateIssueAsync_WhenAssigneeDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        const int projectId = 1;
        const int userId = 10;
        var project = CreateProject(projectId, userId);
        var dto = new CreateIssueDto("New feature", "Description", Priority.Medium, 999);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.CreateIssueAsync(projectId, userId, dto));

        _issueRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Issue>()), Times.Never);
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
        var dto = new CreateIssueDto("New feature", "Description", Priority.Medium, assigneeId);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(assigneeId)).ReturnsAsync(assignee);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => _sut.CreateIssueAsync(projectId, userId, dto));

        _issueRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Issue>()), Times.Never);
    }

    [Fact]
    public async Task CreateIssueAsync_WhenCreatorIsNotProjectMember_ThrowsNotFoundException()
    {
        // Arrange
        const int projectId = 1;
        var project = CreateProject(projectId, 999);
        var dto = new CreateIssueDto("New feature", "Description", Priority.Medium, null);

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.CreateIssueAsync(projectId, 10, dto));

        _issueRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Issue>()), Times.Never);
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
}