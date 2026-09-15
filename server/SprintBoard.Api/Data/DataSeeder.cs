using Microsoft.EntityFrameworkCore;
using SprintBoard.Api.Models;

namespace SprintBoard.Api.Data;

public static class DataSeeder
{
    private const string DemoPassword = "Password123!";

    private static DateTime _lastSeedUtc = DateTime.MinValue;

    private static readonly TimeSpan MinInterval =
        TimeSpan.FromMinutes(5);

    private static readonly SemaphoreSlim SeedLock = new(1, 1);

    /// <summary>
    /// Reseeds only if more than MinInterval has passed since the last reseed.
    /// Safe to call on every page load without wiping data out from under an active visitor.
    /// </summary>
    public static async Task SeedIfDueAsync(AppDbContext context)
    {
        if (DateTime.UtcNow - _lastSeedUtc < MinInterval)
            return;

        await SeedLock.WaitAsync();

        try
        {
            if (DateTime.UtcNow - _lastSeedUtc < MinInterval)
                return;

            await SeedAsync(context);

            _lastSeedUtc = DateTime.UtcNow;
        }
        finally
        {
            SeedLock.Release();
        }
    }

    public static async Task SeedAsync(AppDbContext context)
    {
        Console.WriteLine("[Seeder] Clearing existing data...");

        // Reset all demo data and PostgreSQL identity sequences.
        //
        // RESTART IDENTITY means IDs start from 1 again.
        // CASCADE handles foreign-key dependencies.
        await context.Database.ExecuteSqlRawAsync("""
            TRUNCATE TABLE
                "Comments",
                "Issues",
                "Sprints",
                "ProjectMembers",
                "Projects",
                "Users"
            RESTART IDENTITY CASCADE;
            """);

        // USERS
        Console.WriteLine("[Seeder] Seeding users...");

        var users = new List<User>
        {
            new()
            {
                Name = "Alice Admin",
                Email = "alice@sprintboard.dev",
                Role = UserRole.Admin,
                PasswordHash = Hash(DemoPassword),
                AvatarUrl = CreateAvatarUrl("blobs", "alice-admin")
            },

            new()
            {
                Name = "Bob Builder",
                Email = "bob@sprintboard.dev",
                Role = UserRole.User,
                PasswordHash = Hash(DemoPassword),
                AvatarUrl = CreateAvatarUrl("blobs", "bob-builder")
            },

            new()
            {
                Name = "Carol Coder",
                Email = "carol@sprintboard.dev",
                Role = UserRole.User,
                PasswordHash = Hash(DemoPassword),
                AvatarUrl = CreateAvatarUrl("glass", "carol-coder")
            },

            new()
            {
                Name = "Dave Dev",
                Email = "dave@sprintboard.dev",
                Role = UserRole.User,
                PasswordHash = Hash(DemoPassword),
                AvatarUrl = CreateAvatarUrl("glass", "dave-dev")
            },

            new()
            {
                Name = "Erin Engineer",
                Email = "erin@sprintboard.dev",
                Role = UserRole.User,
                PasswordHash = Hash(DemoPassword),
                AvatarUrl = CreateAvatarUrl("initial-face", "erin-engineer")
            },

            new()
            {
                Name = "Frank Frontend",
                Email = "frank@sprintboard.dev",
                Role = UserRole.User,
                PasswordHash = Hash(DemoPassword),
                AvatarUrl = CreateAvatarUrl("initial-face", "frank-frontend")
            }
        };

        context.Users.AddRange(users);

        await context.SaveChangesAsync();

        var alice = users[0];
        var bob = users[1];
        var carol = users[2];
        var dave = users[3];
        var erin = users[4];
        var frank = users[5];

        // PROJECT
        Console.WriteLine("[Seeder] Seeding project...");

        var project = new Project
        {
            Name = "SprintBoard MVP",
            Description = "Demo project used for local development and testing",
            Icon = CreateAvatarUrl("glass", "sprintboard-mvp")
        };

        context.Projects.Add(project);

        await context.SaveChangesAsync();


        // PROJECT MEMBERS
        Console.WriteLine("[Seeder] Seeding project members...");

        var members = new List<ProjectMember>
        {
            new()
            {
                ProjectId = project.Id,
                UserId = alice.Id,
                ProjectRole = ProjectRole.Owner
            },

            new()
            {
                ProjectId = project.Id,
                UserId = bob.Id,
                ProjectRole = ProjectRole.Member,
                InvitedByUserId = alice.Id
            },

            new()
            {
                ProjectId = project.Id,
                UserId = carol.Id,
                ProjectRole = ProjectRole.Member,
                InvitedByUserId = alice.Id
            },

            new()
            {
                ProjectId = project.Id,
                UserId = dave.Id,
                ProjectRole = ProjectRole.Member,
                InvitedByUserId = alice.Id
            },

            new()
            {
                ProjectId = project.Id,
                UserId = erin.Id,
                ProjectRole = ProjectRole.Member,
                InvitedByUserId = alice.Id
            },

            new()
            {
                ProjectId = project.Id,
                UserId = frank.Id,
                ProjectRole = ProjectRole.Member,
                InvitedByUserId = alice.Id
            }
        };

        context.ProjectMembers.AddRange(members);

        await context.SaveChangesAsync();

        // SPRINTS
        Console.WriteLine("[Seeder] Seeding sprints...");

        var now = DateTime.UtcNow;

        var sprint1 = new Sprint
        {
            ProjectId = project.Id,
            Name = "Sprint 1",
            Goal = "Set up the core project infrastructure",
            StartDate = now.AddDays(-14),
            EndDate = now.AddDays(-7),
            Status = SprintStatus.Completed
        };

        var sprint2 = new Sprint
        {
            ProjectId = project.Id,
            Name = "Sprint 2",
            Goal = "Build the main issue management features",
            StartDate = now.AddDays(-6),
            EndDate = now.AddDays(7),
            Status = SprintStatus.Active
        };

        var sprint3 = new Sprint
        {
            ProjectId = project.Id,
            Name = "Sprint 3",
            Goal = "Improve reporting and polish the application",
            StartDate = now.AddDays(8),
            EndDate = now.AddDays(21),
            Status = SprintStatus.Planned
        };

        context.Sprints.AddRange(
            sprint1,
            sprint2,
            sprint3);

        await context.SaveChangesAsync();

        // ISSUES
        Console.WriteLine("[Seeder] Seeding issues...");

        var assignees = new[]
        {
            alice,
            bob,
            carol,
            dave,
            erin,
            frank
        };

        var statuses = Enum.GetValues<IssueStatus>();
        var priorities = Enum.GetValues<Priority>();

        var issueTitles = new[]
        {
            "Set up CI pipeline",
            "Add GitHub Actions for build + test",
            "Design database schema for issues",
            "Implement JWT authentication",
            "Create project creation endpoint",
            "Build Kanban board UI",
            "Add drag-and-drop for issue cards",
            "Fix pagination bug on project list",
            "Add email notifications for assignments",
            "Write integration tests for ProjectMember",
            "Set up Postgres row-versioning for concurrency",
            "Add dark mode toggle",
            "Refactor IssueService for testability",
            "Add filtering by assignee",
            "Add filtering by priority",
            "Set up Swagger docs",
            "Add rate limiting middleware",
            "Improve error handling in AuthController",
            "Add avatar upload support",
            "Write seed script for demo data"
        };

        var random = new Random(42);

        var issues = new List<Issue>();

        for (int i = 0; i < issueTitles.Length; i++)
        {
            var createdBy =
                assignees[random.Next(assignees.Length)];

            var assignee =
                random.Next(0, 5) == 0
                    ? null
                    : assignees[random.Next(assignees.Length)];

            // Roughly distribute issues:
            int? sprintId = i switch
            {
                < 5 => sprint1.Id,
                < 13 => sprint2.Id,
                < 16 => sprint3.Id,
                _ => null
            };

            issues.Add(new Issue
            {
                ProjectId = project.Id,
                SprintId = sprintId,
                Name = issueTitles[i],
                Description =
                    $"Auto-generated demo issue #{i + 1}",
                Status =
                    statuses[random.Next(statuses.Length)],
                Priority =
                    priorities[random.Next(priorities.Length)],
                CreatedById = createdBy.Id,
                AssigneeId = assignee?.Id,
                CreatedAt =
                    DateTime.UtcNow.AddDays(
                        -random.Next(1, 30))
            });
        }

        context.Issues.AddRange(issues);

        await context.SaveChangesAsync();

        // COMMENTS
        Console.WriteLine("[Seeder] Seeding comments...");

        var sampleComments = new[]
        {
            "Started looking into this, will update soon.",
            "Can we get a bit more detail on the acceptance criteria?",
            "Blocked on the staging environment being down.",
            "This should be ready for review by EOD.",
            "Nice work on this — tested locally and it looks solid.",
            "Reopening, saw a regression on the latest deploy.",
            "Moved this to in review, @assignee please take a look.",
            "Do we need a migration for this or is it just a UI change?",
            "Left a few comments on the PR, nothing blocking.",
            "Marking as done, verified in staging."
        };

        var comments = new List<Comment>();

        foreach (var issue in issues)
        {
            var commentCount = random.Next(0, 4);

            for (int c = 0; c < commentCount; c++)
            {
                var author =
                    assignees[random.Next(assignees.Length)];

                var createdAt =
                    issue.CreatedAt.AddHours(
                        random.Next(1, 72));

                comments.Add(new Comment
                {
                    IssueId = issue.Id,
                    AuthorId = author.Id,
                    Content =
                        sampleComments[
                            random.Next(sampleComments.Length)],
                    CreatedAt =
                        createdAt > DateTime.UtcNow
                            ? DateTime.UtcNow
                            : createdAt
                });
            }
        }

        context.Comments.AddRange(comments);

        await context.SaveChangesAsync();

        Console.WriteLine("[Seeder] Seeding complete.");
    }

    // HELPERS
    private static string CreateAvatarUrl(
        string style,
        string seed)
    {
        var encodedSeed =
            Uri.EscapeDataString(seed);

        return
            $"https://api.dicebear.com/10.x/{style}/svg" +
            $"?seed={encodedSeed}" +
            "&animationVariant=medium";
    }

    private static string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password);
}