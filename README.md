# SprintBoard

SprintBoard is a full-stack project and task management application, inspired by Jira. It allows users to create and manage projects, collaborate with other project members, and track tasks through a Kanban-style workflow.

**Live Demo:** [calm-tree-0f811550f.6.azurestaticapps.net](https://calm-tree-0f811550f.6.azurestaticapps.net/)

---

## Demo Accounts

The live demo is seeded with the following test accounts (all use the same password):

| Email                 | Role  | Password       |
| --------------------- | ----- | -------------- |
| alice@sprintboard.dev | Admin | `Password123!` |
| bob@sprintboard.dev   | User  | `Password123!` |
| carol@sprintboard.dev | User  | `Password123!` |
| dave@sprintboard.dev  | User  | `Password123!` |
| erin@sprintboard.dev  | User  | `Password123!` |
| frank@sprintboard.dev | User  | `Password123!` |

Alice is the owner of the seeded "SprintBoard MVP" project; the rest are project members with sample issues assigned across them.

> Demo accounts only — not representative of production password policy.

### ⚠️ Known Limitation: Login on Brave (and other strict browsers)

The frontend and API are hosted on separate Azure domains (Static Web Apps and App Service). Because the authentication cookie is set by the API domain but read by the frontend domain, browsers with strict third-party cookie blocking — most notably **Brave with Shields enabled**, and Safari's ITP — will block it, even though it's correctly configured with `SameSite=None; Secure`.

- **Workaround:** disable Brave Shields for this site, or use Chrome/Firefox, when testing the live demo.
- **Root cause:** Brave enforces its own third-party cookie blocking layer independently of the `SameSite` cookie spec. Since the frontend and API sit on different origins, the cookie set during login/OAuth is treated as third-party and dropped.
- **Proper fix:** serve the frontend and API from the same domain — Azure Static Web Apps supports this via its "linked backends" feature, which proxies `/api/*` requests through the frontend's origin so the cookie becomes first-party. This requires the **Standard tier** of Azure Static Web Apps; upgrading from the Free tier wasn't worth it for the scope of this project, so the limitation is documented here instead.

---

## Project Status

The core application is functionally complete: authentication, project management, and issue tracking work end-to-end across both the API and the React frontend, including a Kanban board with drag-and-drop status updates, issue CRUD, and an assignee picker backed by project membership.

**Remaining before the project is considered finished:**

- Admin-only routes
- Inline code documentation/comments
- Sprint functionality
- Real-time updates via SignalR

---

## Tech Stack

**Backend**

- C# / ASP.NET Core Web API
- Entity Framework Core
- JWT authentication, stored in HttpOnly cookies
- Google OAuth
- Global exception handling via custom try/catch middleware
- DTO-based API structure
- REST API

**Frontend**

- React + TypeScript
- Tailwind CSS
- DiceBear for generated user avatars (no image upload/storage needed)
- Standard `async/await` for API requests (see [Planned Improvements](#planned-improvements))
- SignalR planned for real-time behaviour

---

## Authentication

SprintBoard uses JWT authentication with the token stored in an HttpOnly `access_token` cookie, issued on login or registration. Because the cookie is HttpOnly, the JWT cannot be accessed via JavaScript, reducing the risk of token theft through client-side scripts (e.g. XSS).

### Routes

| Method | Route                       | Description                                  |
| ------ | --------------------------- | -------------------------------------------- |
| POST   | `/api/Auth/register`        | Register a new user                          |
| POST   | `/api/Auth/login`           | Log in with credentials                      |
| GET    | `/api/Auth/google`          | Start Google OAuth login                     |
| GET    | `/api/Auth/google/complete` | Complete Google OAuth login                  |
| GET    | `/api/Auth/me`              | Get the currently authenticated user         |
| POST   | `/api/Auth/Logout`          | Log out and remove the authentication cookie |

### Flow

```text
Register/Login
      │
      ▼
Frontend sends authentication request
      │
      ▼
API validates credentials → JWT generated
      │
      ▼
JWT stored in HttpOnly cookie
      │
      ▼
Authenticated requests → JWT read from cookie → user identity established
```

Google OAuth follows the same pattern via `/api/Auth/google` → Google → `/api/Auth/google/complete`, after which the user is created/found, a JWT is issued, and the frontend is redirected into the app.

---

## Projects

Users are linked to projects through **Project Members**, so a project can have multiple users, each with a project-level role (e.g. Owner, Member) used for permissions.

| Method | Route                                             | Description                                |
| ------ | ------------------------------------------------- | ------------------------------------------ |
| POST   | `/api/Project/CreateProject`                      | Create a new project                       |
| GET    | `/api/Project/MyProjects`                         | Get projects belonging to the current user |
| GET    | `/api/Project/{id}`                               | Get a project by ID                        |
| PUT    | `/api/Project/{projectId}`                        | Update a project                           |
| DELETE | `/api/Project/{projectId}`                        | Delete a project                           |
| GET    | `/api/Project/{projectId}/members`                | Get a project's members                    |
| POST   | `/api/Project/{projectId}/members`                | Add a member to a project by email         |
| DELETE | `/api/Project/{projectId}/members/{targetUserId}` | Remove a member from a project             |

---

## Issues

Issues belong to a project and move through a status pipeline: `Todo → InProgress → InReview → Done`. New issues always start as `Todo`; status changes afterward via update.

| Method | Route                                        | Description                  |
| ------ | -------------------------------------------- | ---------------------------- |
| GET    | `/api/projects/{projectId}/issues`           | Get all issues for a project |
| GET    | `/api/projects/{projectId}/issues/{issueId}` | Get a single issue by ID     |
| POST   | `/api/projects/{projectId}/issues`           | Create a new issue           |
| PUT    | `/api/projects/{projectId}/issues/{issueId}` | Update an issue              |
| DELETE | `/api/projects/{projectId}/issues/{issueId}` | Delete an issue              |

Only project members can view or modify a project's issues. Assigning an issue to a user requires that user to be a member of the same project.

> **Note:** the `/api/Project/...` and `/api/projects/{projectId}/issues` routes use inconsistent casing/conventions (PascalCase-verb-style vs. lowercase-RESTful). This is a known inconsistency to be cleaned up rather than an intentional design choice.

---

## Users

| Method | Route            | Description           |
| ------ | ---------------- | --------------------- |
| GET    | `/api/User/{id}` | Retrieve a user by ID |

---

## Kanban Board (Frontend)

The board fetches a project's issues and members on load and renders one column per issue status. Dragging a card to a different column updates the UI optimistically and sends a `PUT` request to persist the change, rolling back locally if the request fails.

Issues can be created, edited, and deleted from a modal reachable from the board, including selecting an assignee from the project's member list.

---

## Error Handling

The API uses global exception-handling middleware (built around centralized try/catch logic) so that errors thrown from services are converted into consistent HTTP responses instead of leaking unhandled exceptions. Known exception types (e.g. not-found, forbidden) are mapped to their corresponding HTTP status codes centrally, rather than being caught individually in each controller action.

---

## DTOs

The API uses DTOs to control the data sent between client and server rather than exposing database models directly.

**Authentication:** `LoginDto`, `RegisterDto`, `UserDto`, `UpdateUserDto`
**Projects:** `ProjectDto`, `ProjectResponseDto`, `AddMemberDto`, `ProjectMemberDto`
**Issues:** `CreateIssueDto`, `UpdateIssueDto`, `AssigneeDto`, `IssueResponseDto`
**Enums:** `UserRole`, `ProjectRole`, `Priority` (`Low`/`Medium`/`High`), `IssueStatus` (`Todo`/`InProgress`/`InReview`/`Done`) — all serialized as string names, not numeric values, via `JsonStringEnumConverter`.

---

## Data Model

```text
User
 │
 ├── ProjectMember (role: Owner/Member) ── Project
 │                                            │
 │                                            └── Issue (status, priority, assignee, createdBy)
 │
 └── Projects they belong to (via ProjectMember)
```

- A **User** can belong to many **Projects** via the **ProjectMember** join entity, which also carries a project-specific role.
- A **Project** has many **Issues**.
- An **Issue** belongs to one Project, optionally has one Assignee (a User), and always has a CreatedBy User.

---

## Architecture Notes

A few backend setup details worth calling out beyond the route/DTO tables above:

- **Database:** PostgreSQL, hosted via Supabase, accessed through EF Core (`UseNpgsql`). Migrations run automatically on startup (see the note on seeding below).
- **Timestamp interceptor:** a custom `TimestampInterceptor` is registered with the `DbContext` to automatically manage created/updated timestamps at the EF Core save-changes level, rather than setting them manually in each service method.
- **Dual authentication schemes:** the API's default scheme is JWT Bearer (read from the `access_token` HttpOnly cookie via a custom `OnMessageReceived` handler, rather than the `Authorization` header). A second, temporary cookie-based scheme (`"GoogleTemporary"`) is used only to complete the Google OAuth handshake, after which its result is used to issue the app's own JWT and the temporary cookie is signed out.
- **CORS:** locked to a single configured `FrontendUrl` origin with `AllowCredentials()`, since cookie-based auth requires the frontend origin to be explicitly whitelisted rather than using a wildcard.
- **Reverse proxy awareness:** `UseForwardedHeaders` is configured to honour `X-Forwarded-For` / `X-Forwarded-Proto` headers, since Azure App Service sits behind a reverse proxy — without this, things like HTTPS redirection and the `Secure` cookie flag can behave incorrectly in production.
- **Swagger:** only enabled in the Development environment, not exposed on the live deployment.

## CI/CD

GitHub Actions runs on every push and pull request to `main`, with separate jobs for backend and frontend:

- **Backend:** restores and builds the API (`server/SprintBoard.Api`) and the test project (`server/SprintBoard.Tests`) against .NET 10, then runs the test suite with `dotnet test`.
- **Frontend:** installs dependencies with `npm ci`, runs lint, then builds the client (`client/`) with Node 22.

This currently covers build and test verification only — there is no automated deployment step; deploying to Azure is done manually.

> **Note on demo data:** on every application startup, `Program.cs` runs database migrations and then re-seeds the database from scratch (`DataSeeder.SeedAsync`). This means any changes made during a live demo session (new issues, edited projects, etc.) will be reset whenever the Azure App Service restarts. This is intentional for keeping the demo in a known-good state, but worth knowing if you're revisiting the demo after some time away.

---

## Getting Started

> Adjust connection strings, ports, and environment variable names below to match your local `appsettings.Development.json` / `.env` setup.

**Backend**

```bash
cd SprintBoard.Api
dotnet restore
dotnet ef database update
dotnet run
```

**Frontend**

```bash
cd sprintboard-client
npm install
npm run dev
```

You'll need to configure Google OAuth client credentials and a JWT signing key in your local configuration for authentication to work end-to-end.

---

## Roadmap

**Authentication & Users**

- [x] Registration, login, JWT auth, HttpOnly cookies, Google OAuth, current-user endpoint, logout, user roles
- [ ] Admin-only routes
- [ ] Further authorization refinement

**Projects & Issues**

- [x] Project CRUD, project members, project roles/permissions
- [x] Issue CRUD, assignment, status tracking
- [ ] Sprint model, sprint creation, assigning issues to sprints, sprint progress

**Frontend**

- [x] Login/registration pages, auth state handling, project list, Kanban board, issue create/edit/delete modal, assignee picker
- [x] Project member management UI (add/remove members)
- [ ] Sprint management UI

**Real-time**

- [ ] SignalR integration for live board/issue updates

---

## Planned Improvements

- **TanStack Query** — being considered for server-state management (caching, mutations, loading/error states) as the number of API interactions grows. Not yet implemented, and not guaranteed to be added — the current `async/await` approach works for the app's present scope, so this is a "would improve it" item rather than a committed one.
- **Route naming consistency** — unify the `/api/Project/...` and `/api/projects/...` conventions.
- **Same-origin deployment** — resolve the Brave/Safari cookie limitation described above once/if hosting budget allows.
