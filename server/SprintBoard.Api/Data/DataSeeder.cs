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

    private sealed record SprintPlan(
        string Name,
        string Goal,
        SprintStatus Status,
        int Total,
        int Done);

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
        //
        // Six completed sprints (so velocity has a real trend), one active
        // sprint that is ~10 days into a 14-day cycle (so burndown has shape),
        // and one planned sprint. Sprints run back-to-back, 14 days each.
        Console.WriteLine("[Seeder] Seeding sprints...");

        const int sprintLengthDays = 14;

        var now = DateTime.UtcNow;
        var today = now.Date;

        var plans = new List<SprintPlan>
        {
            new("Sprint 1", "Set up the core project infrastructure",   SprintStatus.Completed, Total: 8,  Done: 5),
            new("Sprint 2", "Ship authentication and project management", SprintStatus.Completed, Total: 9,  Done: 7),
            new("Sprint 3", "Build the Kanban board",                   SprintStatus.Completed, Total: 10, Done: 7),
            new("Sprint 4", "Add filtering and real-time updates",      SprintStatus.Completed, Total: 11, Done: 9),
            new("Sprint 5", "Harden the API and improve test coverage", SprintStatus.Completed, Total: 12, Done: 10),
            new("Sprint 6", "Polish the board and backlog experience",  SprintStatus.Completed, Total: 11, Done: 10),
            new("Sprint 7", "Ship sprint reports: burndown and velocity", SprintStatus.Active,  Total: 12, Done: 7),
            new("Sprint 8", "Prepare for launch and stabilise",         SprintStatus.Planned,   Total: 6,  Done: 0)
        };

        var completedCount = plans.Count(p => p.Status == SprintStatus.Completed);

        // The active sprint started 9 days ago, so today is day 10 of 14.
        var activeStart = today.AddDays(-9);
        var firstStart = activeStart.AddDays(-sprintLengthDays * completedCount);

        var sprints = plans
            .Select((plan, i) =>
            {
                var start = firstStart.AddDays(i * sprintLengthDays);

                return new Sprint
                {
                    ProjectId = project.Id,
                    Name = plan.Name,
                    Goal = plan.Goal,
                    StartDate = start,
                    EndDate = start.AddDays(sprintLengthDays - 1),
                    Status = plan.Status
                };
            })
            .ToList();

        context.Sprints.AddRange(sprints);

        await context.SaveChangesAsync();

        // ISSUES
        Console.WriteLine("[Seeder] Seeding issues...");

        var random = new Random(42);

        // Weighted so the board has a few urgent items and lots of mediums,
        // instead of a uniform spread.
        var assigneePool = new[]
        {
            alice,
            bob, bob,
            carol, carol,
            dave, dave,
            erin, erin,
            frank, frank
        };

        User PickUser() => assigneePool[random.Next(assigneePool.Length)];

        Priority RandomPriority()
        {
            var roll = random.Next(100);
            return roll < 25 ? Priority.High
                 : roll < 70 ? Priority.Medium
                 : Priority.Low;
        }

        DateTime Cap(DateTime value) => value > now ? now : value;

        // Later-in-sprint bias: most work lands in the second half, which is
        // what a real burndown looks like (slow start, then a push).
        int SkewedDay() =>
            (int)Math.Round(1 + 12 * Math.Sqrt(random.NextDouble()));

        // Titles: hand-written ones first (they line up with the early
        // sprint goals), then generated ones so we never run out.
        var baseTitles = new[]
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

        var verbs = new[]
        {
            "Add", "Fix", "Refactor", "Improve",
            "Write tests for", "Document", "Optimize"
        };

        var subjects = new[]
        {
            "sprint planning view", "backlog drag-and-drop", "issue detail modal",
            "comment editing", "project settings page", "member invitations",
            "keyboard shortcuts", "issue search", "board column limits",
            "profile page", "notification preferences", "SignalR reconnect handling",
            "session expiry handling", "loading skeletons", "empty states",
            "CSV export", "audit log", "issue labels", "due dates",
            "burndown accuracy", "velocity report", "mobile layout",
            "accessibility labels", "API pagination", "soft-delete cleanup",
            "Google sign-in flow", "role permissions", "sprint completion flow"
        };

        var usedTitles = new HashSet<string>(baseTitles);
        var titleIndex = 0;

        string NextTitle()
        {
            if (titleIndex < baseTitles.Length)
                return baseTitles[titleIndex++];

            while (true)
            {
                var title =
                    $"{verbs[random.Next(verbs.Length)]} {subjects[random.Next(subjects.Length)]}";

                if (usedTitles.Add(title))
                    return title;
            }
        }

        // Scripted completions for the active sprint (day offsets from its
        // start). Slightly behind the ideal line, with a quiet weekend and a
        // push mid-sprint, so the burndown chart has visible character.
        var activeDoneOffsets = new[] { 1, 2, 4, 4, 5, 7, 8 };

        var activeLeftovers = new[]
        {
            IssueStatus.InProgress,
            IssueStatus.InProgress,
            IssueStatus.InReview,
            IssueStatus.InReview,
            IssueStatus.Todo
        };

        // Unfinished work carried out of a completed sprint.
        var leftoverPool = new[]
        {
            IssueStatus.Todo,
            IssueStatus.InProgress,
            IssueStatus.InReview
        };

        var issues = new List<Issue>();

        for (int s = 0; s < sprints.Count; s++)
        {
            var sprint = sprints[s];
            var plan = plans[s];

            var doneOffsets = sprint.Status == SprintStatus.Active
                ? activeDoneOffsets
                : Enumerable.Range(0, plan.Done)
                    .Select(_ => SkewedDay())
                    .OrderBy(d => d)
                    .ToArray();

            for (int j = 0; j < plan.Total; j++)
            {
                var isDone = j < doneOffsets.Length;

                IssueStatus status;
                if (isDone)
                {
                    status = IssueStatus.Done;
                }
                else if (sprint.Status == SprintStatus.Planned)
                {
                    status = IssueStatus.Todo;
                }
                else if (sprint.Status == SprintStatus.Active)
                {
                    status = activeLeftovers[(j - doneOffsets.Length) % activeLeftovers.Length];
                }
                else
                {
                    status = leftoverPool[random.Next(leftoverPool.Length)];
                }

                // Issues are created before their sprint begins (planning),
                // except for the planned sprint, whose start is in the future.
                var createdAt = sprint.Status == SprintStatus.Planned
                    ? now.AddDays(-random.Next(1, 8))
                    : sprint.StartDate
                        .AddDays(-random.Next(1, 6))
                        .AddHours(9 + random.Next(0, 8));

                DateTime? completedAt = isDone
                    ? Cap(sprint.StartDate
                        .AddDays(doneOffsets[j])
                        .AddHours(9 + random.Next(0, 9)))
                    : null;

                DateTime? updatedAt = isDone
                    ? completedAt
                    : sprint.Status == SprintStatus.Planned
                        ? null
                        : Cap(createdAt.AddDays(random.Next(1, 5)));

                // Started work always has an owner; untouched work is
                // sometimes still unassigned.
                User? assignee;
                if (status != IssueStatus.Todo)
                {
                    assignee = PickUser();
                }
                else
                {
                    var unassignedChance =
                        sprint.Status == SprintStatus.Planned ? 60 : 30;

                    assignee = random.Next(100) < unassignedChance
                        ? null
                        : PickUser();
                }

                issues.Add(new Issue
                {
                    ProjectId = project.Id,
                    SprintId = sprint.Id,
                    Name = NextTitle(),
                    Description = $"Demo issue seeded for {sprint.Name}.",
                    Status = status,
                    Priority = RandomPriority(),
                    CreatedById = users[random.Next(users.Count)].Id,
                    AssigneeId = assignee?.Id,
                    CreatedAt = createdAt,
                    UpdatedAt = updatedAt,
                    CompletedAt = completedAt
                });
            }
        }

        // Backlog: not in any sprint, mostly unassigned.
        for (int b = 0; b < 8; b++)
        {
            issues.Add(new Issue
            {
                ProjectId = project.Id,
                SprintId = null,
                Name = NextTitle(),
                Description = "Backlog item waiting to be scheduled.",
                Status = IssueStatus.Todo,
                Priority = RandomPriority(),
                CreatedById = users[random.Next(users.Count)].Id,
                AssigneeId = random.Next(100) < 70 ? null : PickUser().Id,
                CreatedAt = now.AddDays(-random.Next(1, 20))
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

            // Comments never post-date the issue's completion (or now).
            var limit = issue.CompletedAt ?? now;

            for (int c = 0; c < commentCount; c++)
            {
                var author = users[random.Next(users.Count)];

                var createdAt = issue.CreatedAt.AddHours(random.Next(1, 72));

                comments.Add(new Comment
                {
                    IssueId = issue.Id,
                    AuthorId = author.Id,
                    Content = sampleComments[random.Next(sampleComments.Length)],
                    CreatedAt = createdAt > limit ? limit : createdAt
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