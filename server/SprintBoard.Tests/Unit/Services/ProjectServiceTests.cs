using Moq;
using SprintBoard.Api.DTOs;
using SprintBoard.Api.Exceptions;
using SprintBoard.Api.Models;
using SprintBoard.Api.Repositories;
using SprintBoard.Api.Services;

namespace SprintBoard.Tests.Unit.Services;

public class ProjectServiceTests
{
    private readonly Mock<IProjectRepository> _repositoryMock;
    private readonly ProjectService _service;

    public ProjectServiceTests()
    {
        _repositoryMock = new Mock<IProjectRepository>();
        _service = new ProjectService(_repositoryMock.Object);
    }

    // CreateProjectAsync

    [Fact]
    public async Task CreateProjectAsync_WithValidData_CreatesProject()
    {
        // Arrange
        var userId = 1;

        var dto = new ProjectDto(
            "SprintBoard",
            "Project management application",
            "icon.png"
        );

        var createdProject = new Project
        {
            Id = 1,
            Name = "SprintBoard",
            Description = "Project management application",
            Icon = "icon.png"
        };

        _repositoryMock
            .Setup(r => r.CreateProjectAsync(
                userId,
                It.IsAny<Project>()))
            .ReturnsAsync(createdProject);

        // Act
        var result = await _service.CreateProjectAsync(
            userId,
            dto);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(1, result.Id);
        Assert.Equal("SprintBoard", result.Name);
        Assert.Equal(
            "Project management application",
            result.Description);
        Assert.Equal("icon.png", result.Icon);

        _repositoryMock.Verify(
            r => r.CreateProjectAsync(
                userId,
                It.Is<Project>(p =>
                    p.Name == dto.Name &&
                    p.Description == dto.Description &&
                    p.Icon == dto.Icon)),
            Times.Once);
    }

    // GetUserProjectsAsync

    [Fact]
    public async Task GetUserProjectsAsync_WithProjects_ReturnsProjects()
    {
        // Arrange
        var userId = 1;
        string? search = null;

        var projects = new List<Project>
        {
            new Project
            {
                Id = 1,
                Name = "Project One",
                Description = "First project",
                Icon = "one.png"
            },

            new Project
            {
                Id = 2,
                Name = "Project Two",
                Description = "Second project",
                Icon = "two.png"
            }
        };

        _repositoryMock
            .Setup(r => r.GetUserProjectsAsync(userId, search))
            .ReturnsAsync(projects);

        // Act
        var result = await _service.GetUserProjectsAsync(userId, search);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Assert.Equal(1, result[0].Id);
        Assert.Equal("Project One", result[0].Name);
        Assert.Equal("First project", result[0].Description);
        Assert.Equal("one.png", result[0].Icon);

        Assert.Equal(2, result[1].Id);
        Assert.Equal("Project Two", result[1].Name);
        Assert.Equal("Second project", result[1].Description);
        Assert.Equal("two.png", result[1].Icon);

        _repositoryMock.Verify(
            r => r.GetUserProjectsAsync(userId, search),
            Times.Once);
    }


    [Fact]
    public async Task GetUserProjectsAsync_WhenUserHasNoProjects_ReturnsEmptyList()
    {
        // Arrange
        var userId = 1;
        string? search = null;

        _repositoryMock
            .Setup(r => r.GetUserProjectsAsync(userId, search))
            .ReturnsAsync(new List<Project>());

        // Act
        var result = await _service.GetUserProjectsAsync(userId, search);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

        _repositoryMock.Verify(
            r => r.GetUserProjectsAsync(userId, search),
            Times.Once);
    }


    [Fact]
    public async Task GetUserProjectsAsync_WithSearchTerm_PassesSearchToRepository()
    {
        // Arrange
        var userId = 1;
        var search = "Sprint";

        var projects = new List<Project>
        {
            new Project
            {
                Id = 1,
                Name = "SprintBoard",
                Description = "Project management application",
                Icon = "icon.png"
            }
        };

        _repositoryMock
            .Setup(r => r.GetUserProjectsAsync(userId, search))
            .ReturnsAsync(projects);

        // Act
        var result = await _service.GetUserProjectsAsync(userId, search);

        // Assert
        Assert.Single(result);
        Assert.Equal("SprintBoard", result[0].Name);

        _repositoryMock.Verify(
            r => r.GetUserProjectsAsync(userId, search),
            Times.Once);
    }


    // GetProjectByIdAsync

    [Fact]
    public async Task GetProjectByIdAsync_WhenProjectExistsAndUserIsMember_ReturnsProject()
    {
        // Arrange
        var projectId = 1;
        var userId = 10;

        var project = new Project
        {
            Id = projectId,
            Name = "SprintBoard",
            Description = "Project management application",
            Icon = "icon.png",

            ProjectMembers = new List<ProjectMember>
            {
                new ProjectMember
                {
                    UserId = userId,
                    ProjectRole = ProjectRole.Member
                }
            }
        };

        _repositoryMock
            .Setup(r => r.GetProjectByIdAsync(projectId))
            .ReturnsAsync(project);

        // Act
        var result = await _service.GetProjectByIdAsync(
            projectId,
            userId);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(projectId, result.Id);
        Assert.Equal("SprintBoard", result.Name);
        Assert.Equal(
            "Project management application",
            result.Description);
        Assert.Equal("icon.png", result.Icon);

        _repositoryMock.Verify(
            r => r.GetProjectByIdAsync(projectId),
            Times.Once);
    }


    [Fact]
    public async Task GetProjectByIdAsync_WhenProjectDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var projectId = 999;
        var userId = 1;

        _repositoryMock
            .Setup(r => r.GetProjectByIdAsync(projectId))
            .ReturnsAsync((Project?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.GetProjectByIdAsync(
                projectId,
                userId));

        _repositoryMock.Verify(
            r => r.GetProjectByIdAsync(projectId),
            Times.Once);
    }


    [Fact]
    public async Task GetProjectByIdAsync_WhenUserIsNotMember_ThrowsNotFoundException()
    {
        // Arrange
        var projectId = 1;
        var userId = 999;

        var project = new Project
        {
            Id = projectId,
            Name = "Private Project",

            ProjectMembers = new List<ProjectMember>
            {
                new ProjectMember
                {
                    UserId = 1,
                    ProjectRole = ProjectRole.Owner
                }
            }
        };

        _repositoryMock
            .Setup(r => r.GetProjectByIdAsync(projectId))
            .ReturnsAsync(project);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.GetProjectByIdAsync(
                projectId,
                userId));

        _repositoryMock.Verify(
            r => r.GetProjectByIdAsync(projectId),
            Times.Once);
    }


    // DeleteProjectAsync
    [Fact]
    public async Task DeleteProjectAsync_WhenUserIsOwner_DeletesProject()
    {
        // Arrange
        var userId = 1;
        var projectId = 10;

        var project = new Project
        {
            Id = projectId,
            Name = "SprintBoard",

            ProjectMembers = new List<ProjectMember>
            {
                new ProjectMember
                {
                    UserId = userId,
                    ProjectRole = ProjectRole.Owner
                }
            }
        };

        _repositoryMock
            .Setup(r => r.GetProjectByIdAsync(projectId))
            .ReturnsAsync(project);

        // Act
        await _service.DeleteProjectAsync(
            userId,
            projectId);

        // Assert
        _repositoryMock.Verify(
            r => r.DeleteProjectAsync(project),
            Times.Once);
    }


    [Fact]
    public async Task DeleteProjectAsync_WhenProjectDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var userId = 1;
        var projectId = 999;

        _repositoryMock
            .Setup(r => r.GetProjectByIdAsync(projectId))
            .ReturnsAsync((Project?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.DeleteProjectAsync(
                userId,
                projectId));

        _repositoryMock.Verify(
            r => r.DeleteProjectAsync(
                It.IsAny<Project>()),
            Times.Never);
    }


    [Fact]
    public async Task DeleteProjectAsync_WhenUserIsNotMember_ThrowsForbiddenException()
    {
        // Arrange
        var userId = 999;
        var projectId = 1;

        var project = new Project
        {
            Id = projectId,
            Name = "SprintBoard",

            ProjectMembers = new List<ProjectMember>
            {
                new ProjectMember
                {
                    UserId = 1,
                    ProjectRole = ProjectRole.Owner
                }
            }
        };

        _repositoryMock
            .Setup(r => r.GetProjectByIdAsync(projectId))
            .ReturnsAsync(project);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => _service.DeleteProjectAsync(
                userId,
                projectId));

        _repositoryMock.Verify(
            r => r.DeleteProjectAsync(
                It.IsAny<Project>()),
            Times.Never);
    }


    [Fact]
    public async Task DeleteProjectAsync_WhenUserIsMemberButNotOwner_ThrowsForbiddenException()
    {
        // Arrange
        var userId = 2;
        var projectId = 1;

        var project = new Project
        {
            Id = projectId,
            Name = "SprintBoard",

            ProjectMembers = new List<ProjectMember>
            {
                new ProjectMember
                {
                    UserId = userId,
                    ProjectRole = ProjectRole.Member
                }
            }
        };

        _repositoryMock
            .Setup(r => r.GetProjectByIdAsync(projectId))
            .ReturnsAsync(project);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => _service.DeleteProjectAsync(
                userId,
                projectId));

        _repositoryMock.Verify(
            r => r.DeleteProjectAsync(
                It.IsAny<Project>()),
            Times.Never);
    }


    // UpdateProjectAsync

    [Fact]
    public async Task UpdateProjectAsync_WhenUserIsMember_UpdatesProject()
    {
        // Arrange
        var userId = 1;
        var projectId = 10;

        var project = new Project
        {
            Id = projectId,
            Name = "Old Name",
            Description = "Old description",
            Icon = "old.png",

            ProjectMembers = new List<ProjectMember>
            {
                new ProjectMember
                {
                    UserId = userId,
                    ProjectRole = ProjectRole.Member
                }
            }
        };

        var dto = new ProjectDto(
            "New Name",
            "New description",
            "new.png"
        );

        _repositoryMock
            .Setup(r => r.GetProjectByIdAsync(projectId))
            .ReturnsAsync(project);

        _repositoryMock
            .Setup(r => r.UpdateProjectAsync(project))
            .ReturnsAsync(project);

        // Act
        var result = await _service.UpdateProjectAsync(
            userId,
            projectId,
            dto);

        // Assert

        // Response DTO
        Assert.Equal(projectId, result.Id);
        Assert.Equal("New Name", result.Name);
        Assert.Equal(
            "New description",
            result.Description);
        Assert.Equal("new.png", result.Icon);

        // Entity was updated
        Assert.Equal("New Name", project.Name);
        Assert.Equal(
            "New description",
            project.Description);
        Assert.Equal("new.png", project.Icon);

        // Repository was called
        _repositoryMock.Verify(
            r => r.UpdateProjectAsync(project),
            Times.Once);
    }


    [Fact]
    public async Task UpdateProjectAsync_WhenProjectDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var userId = 1;
        var projectId = 999;

        var dto = new ProjectDto(
            "New Name",
            "New description",
            "new.png"
        );

        _repositoryMock
            .Setup(r => r.GetProjectByIdAsync(projectId))
            .ReturnsAsync((Project?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.UpdateProjectAsync(
                userId,
                projectId,
                dto));

        _repositoryMock.Verify(
            r => r.UpdateProjectAsync(
                It.IsAny<Project>()),
            Times.Never);
    }


    [Fact]
    public async Task UpdateProjectAsync_WhenUserIsNotMember_ThrowsForbiddenException()
    {
        // Arrange
        var userId = 999;
        var projectId = 1;

        var project = new Project
        {
            Id = projectId,
            Name = "SprintBoard",

            ProjectMembers = new List<ProjectMember>
            {
                new ProjectMember
                {
                    UserId = 1,
                    ProjectRole = ProjectRole.Owner
                }
            }
        };

        var dto = new ProjectDto(
            "New Name",
            "New description",
            "new.png"
        );

        _repositoryMock
            .Setup(r => r.GetProjectByIdAsync(projectId))
            .ReturnsAsync(project);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => _service.UpdateProjectAsync(
                userId,
                projectId,
                dto));

        _repositoryMock.Verify(
            r => r.UpdateProjectAsync(
                It.IsAny<Project>()),
            Times.Never);
    }
}