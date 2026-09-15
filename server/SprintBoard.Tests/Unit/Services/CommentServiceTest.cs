using Microsoft.AspNetCore.Http;
using Moq;
using SprintBoard.Api.DTOs;
using SprintBoard.Api.Exceptions;
using SprintBoard.Api.Models;
using SprintBoard.Api.Repositories;
using SprintBoard.Api.Services;
using Xunit;

namespace SprintBoard.Api.Tests.Services;

public class CommentServiceTests
{
    private readonly Mock<ICommentRepository> _commentRepositoryMock;
    private readonly Mock<IIssueRepository> _issueRepositoryMock;
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly CommentService _sut;

    private const int DefaultProjectId = 10;
    private const int DefaultIssueId = 100;
    private const int DefaultUserId = 1;

    public CommentServiceTests()
    {
        _commentRepositoryMock = new Mock<ICommentRepository>();
        _issueRepositoryMock = new Mock<IIssueRepository>();
        _projectRepositoryMock = new Mock<IProjectRepository>();

        _sut = new CommentService(
            _commentRepositoryMock.Object,
            _issueRepositoryMock.Object,
            _projectRepositoryMock.Object
        );
    }

    // --- Helper Setup Methods ---

    private void SetupValidContext(ProjectRole role = ProjectRole.Member)
    {
        var issue = new Issue { Id = DefaultIssueId, ProjectId = DefaultProjectId };
        var project = new Project
        {
            Id = DefaultProjectId,
            ProjectMembers = new List<ProjectMember>
            {
                new() { UserId = DefaultUserId, ProjectRole = role }
            }
        };

        _issueRepositoryMock.Setup(r => r.GetByIdAsync(DefaultIssueId)).ReturnsAsync(issue);
        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(DefaultProjectId)).ReturnsAsync(project);
    }

    private static User CreateSampleUser(int id = DefaultUserId, string name = "Ada Lovelace") => new()
    {
        Id = id,
        Name = name,
        AvatarUrl = "https://example.com/avatar.png"
    };

    // --- GetCommentsAsync Tests ---

    [Fact]
    public async Task GetCommentsAsync_WhenValidRequest_ReturnsMappedDtos()
    {
        // Arrange
        SetupValidContext();
        var author = CreateSampleUser();
        var comments = new List<Comment>
        {
            new() { Id = 1, IssueId = DefaultIssueId, Content = "First comment", Author = author, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, IssueId = DefaultIssueId, Content = "Second comment", Author = author, CreatedAt = DateTime.UtcNow }
        };

        _commentRepositoryMock.Setup(r => r.GetByIssueIdAsync(DefaultIssueId)).ReturnsAsync(comments);

        // Act
        var result = await _sut.GetCommentsAsync(DefaultProjectId, DefaultIssueId, DefaultUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("First comment", result[0].Content);
        Assert.Equal(author.Id, result[0].Author.Id);
        Assert.Equal(author.Name, result[0].Author.Name);
    }

    [Fact]
    public async Task GetCommentsAsync_WhenIssueDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _issueRepositoryMock.Setup(r => r.GetByIdAsync(DefaultIssueId)).ReturnsAsync((Issue?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.GetCommentsAsync(DefaultProjectId, DefaultIssueId, DefaultUserId));
    }

    [Fact]
    public async Task GetCommentsAsync_WhenIssueBelongsToAnotherProject_ThrowsNotFoundException()
    {
        // Arrange
        var issue = new Issue { Id = DefaultIssueId, ProjectId = 999 }; // Mismatched project ID
        _issueRepositoryMock.Setup(r => r.GetByIdAsync(DefaultIssueId)).ReturnsAsync(issue);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.GetCommentsAsync(DefaultProjectId, DefaultIssueId, DefaultUserId));
    }

    [Fact]
    public async Task GetCommentsAsync_WhenUserIsNotProjectMember_ThrowsNotFoundException()
    {
        // Arrange
        var issue = new Issue { Id = DefaultIssueId, ProjectId = DefaultProjectId };
        var project = new Project { Id = DefaultProjectId, ProjectMembers = new List<ProjectMember>() };

        _issueRepositoryMock.Setup(r => r.GetByIdAsync(DefaultIssueId)).ReturnsAsync(issue);
        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(DefaultProjectId)).ReturnsAsync(project);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.GetCommentsAsync(DefaultProjectId, DefaultIssueId, DefaultUserId));
    }

    // --- CreateCommentAsync Tests ---

    [Fact]
    public async Task CreateCommentAsync_WhenValidDto_CreatesAndReturnsComment()
    {
        // Arrange
        SetupValidContext();
        var dto = new CreateCommentDto { Content = "  New comment content  " };
        var author = CreateSampleUser();

        var persistedComment = new Comment
        {
            Id = 50,
            IssueId = DefaultIssueId,
            AuthorId = DefaultUserId,
            Content = "New comment content",
            Author = author,
            CreatedAt = DateTime.UtcNow
        };

        _commentRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Comment>())).Returns(Task.CompletedTask);
        _commentRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _commentRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(persistedComment);

        // Act
        var result = await _sut.CreateCommentAsync(DefaultProjectId, DefaultIssueId, DefaultUserId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New comment content", result.Content);
        Assert.Equal(DefaultUserId, result.Author.Id);
        _commentRepositoryMock.Verify(r => r.AddAsync(It.Is<Comment>(c => c.Content == "New comment content")), Times.Once);
        _commentRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(" ")]
    public async Task CreateCommentAsync_WhenContentIsEmptyOrWhitespace_ThrowsBadHttpRequestException(string content)
    {
        // Arrange
        SetupValidContext();
        var dto = new CreateCommentDto { Content = content };

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() =>
            _sut.CreateCommentAsync(DefaultProjectId, DefaultIssueId, DefaultUserId, dto));
    }

    [Fact]
    public async Task CreateCommentAsync_WhenPersistenceFails_ThrowsInvalidOperationException()
    {
        // Arrange
        SetupValidContext();
        var dto = new CreateCommentDto { Content = "Valid content" };

        _commentRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Comment>())).Returns(Task.CompletedTask);
        _commentRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _commentRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Comment?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.CreateCommentAsync(DefaultProjectId, DefaultIssueId, DefaultUserId, dto));
    }

    // --- UpdateCommentAsync Tests ---

    [Fact]
    public async Task UpdateCommentAsync_WhenAuthorUpdatesOwnComment_UpdatesAndReturnsDto()
    {
        // Arrange
        SetupValidContext(ProjectRole.Member);
        var author = CreateSampleUser(DefaultUserId);
        var existingComment = new Comment
        {
            Id = 1,
            IssueId = DefaultIssueId,
            AuthorId = DefaultUserId,
            Content = "Old Content",
            Author = author
        };

        _commentRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingComment);
        _commentRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new UpdateCommentDto { Content = "Updated Content" };

        // Act
        var result = await _sut.UpdateCommentAsync(DefaultProjectId, DefaultIssueId, 1, DefaultUserId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Content", result.Content);
        Assert.NotNull(result.UpdatedAt);
        _commentRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateCommentAsync_WhenOwnerUpdatesOtherUsersComment_Succeeds()
    {
        // Arrange
        SetupValidContext(ProjectRole.Owner); // Current user is Owner
        var author = CreateSampleUser(id: 999);
        var existingComment = new Comment
        {
            Id = 1,
            IssueId = DefaultIssueId,
            AuthorId = 999, // Author is someone else
            Content = "Old Content",
            Author = author
        };

        _commentRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingComment);
        _commentRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new UpdateCommentDto { Content = "Owner Edited Content" };

        // Act
        var result = await _sut.UpdateCommentAsync(DefaultProjectId, DefaultIssueId, 1, DefaultUserId, dto);

        // Assert
        Assert.Equal("Owner Edited Content", result.Content);
    }

    [Fact]
    public async Task UpdateCommentAsync_WhenNonAuthorNonOwnerUpdates_ThrowsForbiddenException()
    {
        // Arrange
        SetupValidContext(ProjectRole.Member); // Current user is standard Member
        var existingComment = new Comment
        {
            Id = 1,
            IssueId = DefaultIssueId,
            AuthorId = 999 // Different user's comment
        };

        _commentRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingComment);
        var dto = new UpdateCommentDto { Content = "Unauthorized Update" };

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _sut.UpdateCommentAsync(DefaultProjectId, DefaultIssueId, 1, DefaultUserId, dto));
    }

    [Fact]
    public async Task UpdateCommentAsync_WhenCommentIdBelongsToDifferentIssue_ThrowsNotFoundException()
    {
        // Arrange
        SetupValidContext();
        var existingComment = new Comment
        {
            Id = 1,
            IssueId = 999, // Mismatched Issue ID
            AuthorId = DefaultUserId
        };

        _commentRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingComment);
        var dto = new UpdateCommentDto { Content = "Valid content" };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.UpdateCommentAsync(DefaultProjectId, DefaultIssueId, 1, DefaultUserId, dto));
    }

    // --- DeleteCommentAsync Tests ---

    [Fact]
    public async Task DeleteCommentAsync_WhenAuthorDeletesOwnComment_RemovesComment()
    {
        // Arrange
        SetupValidContext(ProjectRole.Member);
        var existingComment = new Comment
        {
            Id = 1,
            IssueId = DefaultIssueId,
            AuthorId = DefaultUserId
        };

        _commentRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingComment);
        _commentRepositoryMock.Setup(r => r.Remove(existingComment));
        _commentRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteCommentAsync(DefaultProjectId, DefaultIssueId, 1, DefaultUserId);

        // Assert
        _commentRepositoryMock.Verify(r => r.Remove(existingComment), Times.Once);
        _commentRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteCommentAsync_WhenNonAuthorNonOwnerDeletes_ThrowsForbiddenException()
    {
        // Arrange
        SetupValidContext(ProjectRole.Member);
        var existingComment = new Comment
        {
            Id = 1,
            IssueId = DefaultIssueId,
            AuthorId = 999 // Different user's comment
        };

        _commentRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingComment);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _sut.DeleteCommentAsync(DefaultProjectId, DefaultIssueId, 1, DefaultUserId));
        
        _commentRepositoryMock.Verify(r => r.Remove(It.IsAny<Comment>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCommentAsync_WhenCommentDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        SetupValidContext();
        _commentRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Comment?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.DeleteCommentAsync(DefaultProjectId, DefaultIssueId, 1, DefaultUserId));
    }
}