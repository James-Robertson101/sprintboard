using SprintBoard.Api.Models;

namespace SprintBoard.Api.Data;

public static class DataSeeder
{
    private const string DemoPassword = "Password123!";

    public static async Task SeedAsync(AppDbContext context)
    {
        Console.WriteLine("[Seeder] Clearing existing data...");
        // Delete in dependency order (children first)
        context.Issues.RemoveRange(context.Issues);
        context.ProjectMembers.RemoveRange(context.ProjectMembers);
        context.Projects.RemoveRange(context.Projects);
        context.Users.RemoveRange(context.Users);
        await context.SaveChangesAsync();

        Console.WriteLine("[Seeder] seeding data...");
        var users = new List<User>
        {
            new() { Name = "Alice Admin",  Email = "alice@sprintboard.dev", Role = UserRole.Admin, PasswordHash = Hash(DemoPassword) },
            new() { Name = "Bob Builder",  Email = "bob@sprintboard.dev",   Role = UserRole.User,  PasswordHash = Hash(DemoPassword) },
            new() { Name = "Carol Coder",  Email = "carol@sprintboard.dev", Role = UserRole.User,  PasswordHash = Hash(DemoPassword) },
            new() { Name = "Dave Dev",     Email = "dave@sprintboard.dev",  Role = UserRole.User,  PasswordHash = Hash(DemoPassword) },
            new() { Name = "Erin Engineer",Email = "erin@sprintboard.dev",  Role = UserRole.User,  PasswordHash = Hash(DemoPassword) },
            new() { Name = "Frank Frontend",Email = "frank@sprintboard.dev",Role = UserRole.User,  PasswordHash = Hash(DemoPassword) },
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync(); // generate Ids

        var alice = users[0];
        var bob = users[1];
        var carol = users[2];
        var dave = users[3];
        var erin = users[4];
        var frank = users[5];

        var project = new Project
        {
            Name = "SprintBoard MVP",
            Description = "Demo project used for local development and testing",
            Icon = "🚀"
        };

        context.Projects.Add(project);
        await context.SaveChangesAsync(); // generate project.Id

        var members = new List<ProjectMember>
        {
            new() { ProjectId = project.Id, UserId = alice.Id, ProjectRole = ProjectRole.Owner },
            new() { ProjectId = project.Id, UserId = bob.Id,   ProjectRole = ProjectRole.Member, InvitedByUserId = alice.Id },
            new() { ProjectId = project.Id, UserId = carol.Id, ProjectRole = ProjectRole.Member, InvitedByUserId = alice.Id },
            new() { ProjectId = project.Id, UserId = dave.Id,  ProjectRole = ProjectRole.Member, InvitedByUserId = alice.Id },
            new() { ProjectId = project.Id, UserId = erin.Id,  ProjectRole = ProjectRole.Member, InvitedByUserId = alice.Id },
            new() { ProjectId = project.Id, UserId = frank.Id, ProjectRole = ProjectRole.Member, InvitedByUserId = alice.Id },
        };

        context.ProjectMembers.AddRange(members);
        await context.SaveChangesAsync();

        var assignees = new[] { alice, bob, carol, dave, erin, frank };
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
            "Write seed script for demo data",
        };

        var random = new Random(42); // fixed seed for reproducible demo data
        var issues = new List<Issue>();

        for (int i = 0; i < issueTitles.Length; i++)
        {
            var createdBy = assignees[random.Next(assignees.Length)];
            var assignee = random.Next(0, 5) == 0 ? null : assignees[random.Next(assignees.Length)]; // ~20% unassigned

            issues.Add(new Issue
            {
                ProjectId = project.Id,
                Name = issueTitles[i],
                Description = $"Auto-generated demo issue #{i + 1}",
                Status = statuses[random.Next(statuses.Length)],
                Priority = priorities[random.Next(priorities.Length)],
                CreatedById = createdBy.Id,
                AssigneeId = assignee?.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
            });
        }

        context.Issues.AddRange(issues);
        await context.SaveChangesAsync();
        Console.WriteLine("[Seeder] Seeding complete.");

    }

    private static string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);
}