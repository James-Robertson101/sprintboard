# SprintBoard

A full-stack, real-time project management app inspired by Jira. Teams create projects, plan sprints from a backlog, move issues across a Kanban board, discuss them in comments, and see changes from other members appear instantly. A reports page shows sprint summary, burndown, and velocity.

**Live Demo:** [calm-tree-0f811550f.6.azurestaticapps.net](https://calm-tree-0f811550f.6.azurestaticapps.net/)

<!-- TODO: add a GIF here of two browser windows side by side, dragging a card in one and watching it move in the other. It is the single best thing you can show. -->

---

## Highlights

- **Real-time collaboration** with SignalR: issue creates, updates, and deletes are pushed to every member viewing a project's board.
- **Optimistic UI**: drag-and-drop updates instantly and rolls back if the server rejects the change.
- **Full sprint lifecycle**: backlog, sprint planning, start, and complete, with issues assigned to sprints.
- **Reports**: sprint summary, burndown, and velocity, calculated by issue count (the app has no story points).
- **Issue comments** with create, edit, and delete.
- **Soft-deleted users** filtered globally with an EF Core query filter, so deleting a user never orphans issue history.
- **Cookie-based JWT auth** (HttpOnly) plus Google OAuth, with global roles and project-level roles.
- **CI** on every push and pull request (build, test, lint).

---

## Demo Accounts

The live demo is seeded with these accounts (all use the same password):

| Email                 | Role  | Password       |
| --------------------- | ----- | -------------- |
| alice@sprintboard.dev | Admin | `Password123!` |
| bob@sprintboard.dev   | User  | `Password123!` |
| carol@sprintboard.dev | User  | `Password123!` |
| dave@sprintboard.dev  | User  | `Password123!` |
| erin@sprintboard.dev  | User  | `Password123!` |
| frank@sprintboard.dev | User  | `Password123!` |

Alice owns the seeded "SprintBoard MVP" project; the rest are members with sample issues assigned. To see real-time updates, log in as two different users in two browsers (or one normal and one private window) and open the same project.

> Demo accounts only, not representative of production password policy.

### ⚠️ Known Limitation: Login on Brave (and other strict browsers)

The frontend and API are hosted on separate Azure domains (Static Web Apps and App Service). The auth cookie is set by the API domain but used by the frontend domain, so browsers with strict third-party cookie blocking (Brave with Shields, Safari's ITP) drop it, even with `SameSite=None; Secure`.

- **Workaround:** disable Brave Shields for this site, or use Chrome/Firefox.
- **Root cause:** Brave enforces its own third-party cookie blocking independently of the `SameSite` spec. Because the frontend and API are on different origins, the login/OAuth cookie is treated as third-party.
- **Proper fix:** serve both from one origin. Azure Static Web Apps supports this through "linked backends", which proxy `/api/*` through the frontend's origin and make the cookie first-party. That needs the Standard tier, which wasn't worth it for this project, so the limitation is documented instead.

---

## Tech Stack

**Backend**

- C# / ASP.NET Core Web API (.NET 10)
- Entity Framework Core with PostgreSQL (Supabase)
- SignalR for real-time updates
- JWT auth in HttpOnly cookies, plus Google OAuth
- Centralized exception-handling middleware
- Controller, service, repository layering with DTOs

**Frontend**

- React + TypeScript
- Tailwind CSS
- SignalR client for live updates
- DiceBear for generated avatars (no upload/storage needed)

**Infrastructure**

- Azure App Service (API) and Azure Static Web Apps (client)
- GitHub Actions CI

---

## Architecture

Requests flow through three layers, each registered per-request through dependency injection:

```text
Controller  →  Service  →  Repository  →  AppDbContext (EF Core / PostgreSQL)
(HTTP, DTOs)   (business rules,            (queries, persistence)
               authorization checks)
```

Controllers stay thin: they read the current user's ID from the JWT claims, call a service, and return a DTO. Services enforce the rules (project membership, ownership, valid status changes) and throw typed exceptions, which the global exception middleware converts to HTTP responses. Repositories hold the EF Core queries. The domains are users, auth, projects, issues, sprints, comments, and reports, each with its own service and repository.

---

## Design Decisions

### Real-time updates with SignalR

The hub lives at `/hub/sprintboard` and requires authentication. When a client opens a project it calls `JoinProject(projectId)`, which adds its connection to a `project-{id}` group; it calls `LeaveProject` when navigating away. When an issue is created, updated, or deleted, the change is broadcast to that project's group only, so members of other projects never receive it. Clients apply the change to local board state, so other members see it without refreshing.

Enums are serialized as strings in the SignalR JSON protocol as well as in the REST API, so hub payloads and API responses share the same shape and the client handles one format.

Hub authentication reuses the same HttpOnly `access_token` cookie as the REST API. The JWT bearer handler reads the token from the cookie on every request, including the hub's negotiate and WebSocket connections, so the token never has to be put in a query string (where it would end up in logs).

<!-- TODO: fill in the specifics below.
- Once you've added a project-membership check to JoinProject (see notes), say so here: "JoinProject verifies membership before adding the connection to the group."
- Where are broadcasts sent from (services via IHubContext<SprintBoardHub>)? Do you send the full IssueResponseDto or just an ID + change type?
- How does the sender avoid double-applying its own optimistic update?
-->

### Optimistic drag-and-drop

Dragging a card updates the UI immediately and sends a `PUT` to persist the new status. If the request fails, the change is rolled back locally. Because SignalR also pushes the change to other clients, all connected members converge on the same board state.

### Sprint lifecycle and backlog

Issues without a sprint form the project's **backlog**. Sprints are created, edited, started, and completed through explicit endpoints (`/start` and `/complete`) rather than by editing a status field, so the state transitions are enforced in one place. Issues are moved into a sprint from the backlog, and the board shows the **current** sprint's issues.

<!-- TODO: one or two sentences on the rules. E.g. only one active sprint per project? What happens to unfinished issues when a sprint completes? -->

### Reports: burndown and velocity

The reports page shows a sprint summary, a burndown chart for a single sprint, and velocity across the last few completed sprints (default 6, clamped to 1-20). The app has no story points, so all three use **issue counts** as the unit of work. That's a deliberate simplification rather than a limitation: the charts are only as meaningful as the estimate behind them, and equal-weight issues are honest about that.

<!-- TODO: one or two sentences on how burndown is computed. Status-change history, a Done timestamp, or a daily snapshot? -->

### Soft deletion of users

Users are never physically deleted. An `IsDeleted` flag is set and a global query filter in `AppDbContext` hides them from every query:

```csharp
modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
```

This keeps historical data intact (issues and comments a user authored remain valid) and means individual queries can't forget to exclude deleted users. There are two deletion paths, both going through a `UserDeletionService`: admins can delete any user, and users can delete their own account. If deletion isn't allowed, the API responds `409 Conflict` with a message explaining why. Where a deleted user's data must still be read (for example, showing who created an old issue), the filter can be bypassed explicitly with `IgnoreQueryFilters()`.

<!-- TODO: what does the service block? (e.g. deleting the sole owner of a project, an admin deleting themselves) One sentence here is a good talking point. -->

### Cookie-based JWT with two authentication schemes

The JWT lives in an HttpOnly `access_token` cookie (`Secure`, `SameSite=None`), so client-side JavaScript, and therefore XSS, can't read it. The default scheme is JWT Bearer, but tokens are read from the cookie through a custom `OnMessageReceived` handler rather than the `Authorization` header. A second, temporary cookie scheme (`"GoogleTemporary"`) exists only to complete the Google OAuth handshake; once the user is found or created and the app's own JWT is issued, that cookie is signed out.

### Centralized error handling

Exception-handling middleware maps known exception types (not found, forbidden, and so on) to HTTP status codes in one place, so controllers stay free of try/catch blocks and clients always get consistent error responses.

### Timestamps via an EF Core interceptor

A `TimestampInterceptor` sets created/updated timestamps at the `SaveChanges` level, so no service method has to remember to do it.

### Production concerns

- **CORS** is locked to a single configured `FrontendUrl` with `AllowCredentials()`, since cookie auth doesn't work with a wildcard origin.
- **Forwarded headers** (`X-Forwarded-For` / `X-Forwarded-Proto`) are honoured because Azure App Service sits behind a reverse proxy; without this, HTTPS redirection and the `Secure` cookie flag misbehave.
- **Swagger** is enabled in Development only.

---

## API Reference

Nested resources (issues, sprints, comments, reports) live under `/api/projects/{projectId}/...`. Every endpoint below requires authentication unless noted.

### Authentication and account

| Method | Route                       | Description                                  |
| ------ | --------------------------- | -------------------------------------------- |
| POST   | `/api/Auth/register`        | Register a new user (anonymous)              |
| POST   | `/api/Auth/login`           | Log in with credentials (anonymous)          |
| GET    | `/api/Auth/google`          | Start Google OAuth login (anonymous)         |
| GET    | `/api/Auth/google/complete` | Complete Google OAuth login (anonymous)      |
| GET    | `/api/Auth/me`              | Get the currently authenticated user         |
| PUT    | `/api/Auth/me`              | Update your own profile                      |
| DELETE | `/api/Auth/me`              | Delete (soft-delete) your own account        |
| POST   | `/api/Auth/Logout`          | Log out and remove the authentication cookie |
| DELETE | `/api/Auth/{id}`            | **Admin only:** soft-delete any user         |

### Users

| Method | Route            | Description           |
| ------ | ---------------- | --------------------- |
| GET    | `/api/User/{id}` | Retrieve a user by ID |

### Projects and members

Users join projects through **ProjectMember**, which carries a project-level role (`Owner` / `Member`).

| Method | Route                                              | Description                                        |
| ------ | -------------------------------------------------- | -------------------------------------------------- |
| POST   | `/api/Project/CreateProject`                       | Create a new project                               |
| GET    | `/api/Project/MyProjects?search=`                  | Get your projects, optionally filtered by name     |
| GET    | `/api/Project/{id}`                                | Get a project by ID                                |
| PUT    | `/api/Project/{projectId}`                         | Update a project                                   |
| DELETE | `/api/Project/{projectId}`                         | Delete a project                                   |
| GET    | `/api/Project/{projectId}/members`                 | Get a project's members                            |
| POST   | `/api/Project/{projectId}/members`                 | Add a member by email                              |
| DELETE | `/api/Project/{projectId}/members/{targetUserId}`  | Remove a member                                    |
| GET    | `/api/Project/{projectId}/available-users?search=` | Search users who can still be added to the project |

### Issues and backlog

Issues move through `Todo → InProgress → InReview → Done`. New issues always start as `Todo`. Only project members can view or modify issues, and an issue can only be assigned to a member of the same project.

| Method | Route                                               | Description                         |
| ------ | --------------------------------------------------- | ----------------------------------- |
| GET    | `/api/projects/{projectId}/issues`                  | Get all issues for a project        |
| GET    | `/api/projects/{projectId}/issues/current`          | Get issues in the current sprint    |
| GET    | `/api/projects/{projectId}/issues/{issueId}`        | Get a single issue by ID            |
| POST   | `/api/projects/{projectId}/issues`                  | Create a new issue                  |
| PUT    | `/api/projects/{projectId}/issues/{issueId}`        | Update an issue                     |
| DELETE | `/api/projects/{projectId}/issues/{issueId}`        | Delete an issue                     |
| GET    | `/api/projects/{projectId}/backlog`                 | Get issues not assigned to a sprint |
| PUT    | `/api/projects/{projectId}/issues/{issueId}/sprint` | Assign an issue to a sprint         |

### Sprints

| Method | Route                                                   | Description              |
| ------ | ------------------------------------------------------- | ------------------------ |
| GET    | `/api/projects/{projectId}/sprints`                     | List a project's sprints |
| GET    | `/api/projects/{projectId}/sprints/{sprintId}`          | Get a sprint             |
| POST   | `/api/projects/{projectId}/sprints`                     | Create a sprint          |
| PUT    | `/api/projects/{projectId}/sprints/{sprintId}`          | Update a sprint          |
| POST   | `/api/projects/{projectId}/sprints/{sprintId}/start`    | Start a sprint           |
| POST   | `/api/projects/{projectId}/sprints/{sprintId}/complete` | Complete a sprint        |
| DELETE | `/api/projects/{projectId}/sprints/{sprintId}`          | Delete a sprint          |

### Comments

| Method | Route                                                             | Description              |
| ------ | ----------------------------------------------------------------- | ------------------------ |
| GET    | `/api/projects/{projectId}/issues/{issueId}/comments`             | List an issue's comments |
| POST   | `/api/projects/{projectId}/issues/{issueId}/comments`             | Add a comment            |
| PUT    | `/api/projects/{projectId}/issues/{issueId}/comments/{commentId}` | Edit a comment           |
| DELETE | `/api/projects/{projectId}/issues/{issueId}/comments/{commentId}` | Delete a comment         |

### Reports

| Method | Route                                                           | Description                                   |
| ------ | --------------------------------------------------------------- | --------------------------------------------- |
| GET    | `/api/projects/{projectId}/reports/sprints/{sprintId}/summary`  | Sprint summary                                |
| GET    | `/api/projects/{projectId}/reports/sprints/{sprintId}/burndown` | Burndown data for a sprint                    |
| GET    | `/api/projects/{projectId}/reports/velocity?sprints=6`          | Velocity across recent sprints (1-20 sprints) |

### System

| Method | Route                | Description                                                   |
| ------ | -------------------- | ------------------------------------------------------------- |
| POST   | `/api/system/reseed` | Anonymous. Re-seeds the demo database only if a reseed is due |

### SignalR hub

| Endpoint           | Client method             | Description                               |
| ------------------ | ------------------------- | ----------------------------------------- |
| `/hub/sprintboard` | `JoinProject(projectId)`  | Subscribe to a project's live updates     |
| `/hub/sprintboard` | `LeaveProject(projectId)` | Unsubscribe from a project's live updates |

<!-- TODO: list the server-to-client events (e.g. IssueCreated, IssueUpdated, IssueDeleted) with their payloads. -->

---

## Data Model

```text
User (soft-deletable)
 │
 ├── ProjectMember (role: Owner/Member) ── Project
 │                                            │
 │                                            ├── Sprint
 │                                            │
 │                                            └── Issue (status, priority, assignee, createdBy, sprint)
 │                                                  │
 │                                                  └── Comment (author)
 │
 └── Projects they belong to (via ProjectMember)
```

- A **User** can belong to many **Projects** through **ProjectMember**, which carries a project-specific role.
- A **Project** has many **Issues** and **Sprints**.
- An **Issue** belongs to one Project, optionally has an Assignee and a Sprint (no sprint means it's in the backlog), always has a CreatedBy user, and can have many Comments.

**DTOs** keep database models out of the API surface. Enums (`UserRole`, `ProjectRole`, `Priority`, `IssueStatus`) serialize as strings via `JsonStringEnumConverter`.

---

## CI/CD

GitHub Actions runs on every push and pull request to `main`:

- **Backend:** restores and builds the API and test project against .NET 10, then runs `dotnet test`.
- **Frontend:** `npm ci`, lint, then build with Node 22.

There's no automated deployment; deploys to Azure are manual.

> **Note on demo data:** on every startup `Program.cs` runs migrations and re-seeds the database from scratch (`DataSeeder.SeedAsync`), and `POST /api/system/reseed` re-seeds it again when a reseed is due. Changes made during a live demo therefore reset, which is intentional, to keep the demo in a known-good state.

<!-- TODO: how often is a reseed "due", and what calls the endpoint (the frontend on load? a scheduled job?) -->

---

## Getting Started

> Adjust connection strings, ports, and environment variable names to match your local `appsettings.Development.json` / `.env`.

**Backend**

```bash
cd server/SprintBoard.Api
dotnet restore
dotnet ef database update
dotnet run
```

**Frontend**

```bash
cd client
npm install
npm run dev
```

Required configuration: a `Supabase` connection string, `Jwt:Issuer`, `Jwt:Secret`, `Jwt:ExpiryHours`, `FrontendUrl`, and Google OAuth credentials (`Authentication:Google:ClientId` / `ClientSecret`).

---

## Roadmap

Everything originally planned is implemented, apart from the item below.

- [x] Auth, Google OAuth, roles, and admin-only routes
- [x] Project CRUD, members, and project-level permissions
- [x] Issue CRUD, assignment, status tracking, and comments
- [x] Backlog and sprints (create, start, complete)
- [x] Real-time board updates via SignalR
- [x] Reports: sprint summary, burndown, and velocity
- [x] Soft deletion of users
- [ ] **TanStack Query** for server-state management (caching, mutations, loading/error states). The current `async/await` approach covers the app's needs, so this remains a "would improve it" item.

**Other cleanup**

- Unify the `/api/Project/...` and `/api/projects/...` route conventions (currently inconsistent).
- Move user deletion out of `AuthController` (`DELETE /api/Auth/{id}`) into the user controller.
- Move to same-origin deployment to resolve the Brave/Safari cookie limitation.
