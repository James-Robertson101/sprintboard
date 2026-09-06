# SprintBoard

SprintBoard is a project and task management application currently under development.

The goal of SprintBoard is to allow users to create and manage projects, work with other project members, and manage tasks and sprints within those projects.

## 🚧 Development Status

SprintBoard is currently in active development.

### Completed so far

- JWT authentication
- JWT stored using **HttpOnly cookies**
- User registration
- User login
- Google authentication
- Current-user endpoint
- User and Project models
- Project Members relationship
- Project creation, retrieval, update, and deletion
- Adding and removing project members
- Project roles and project-level permissions
- Issue model
- Creating, updating, and deleting issues
- Assigning issues to project members
- Issue status tracking
- Global exception handling middleware
- Enum values serialized as strings across the API (via `JsonStringEnumConverter`)
- DTO-based API structure
- User roles
- React frontend login and registration pages
- Frontend authentication connected to the API
- Project list page
- Project Kanban board with drag-and-drop status updates
- Issue create/edit modal with priority, status, and assignee fields
- Issue delete with confirmation
- Assignee picker backed by the project members endpoint
- Authentication requests currently handled using standard `async/await`

### Currently working on

- Project member management UI (invite/remove members from the frontend)
- Sprint functionality
- Further project authorization/permissions refinement

### Planned frontend improvements

- Introduce **TanStack Query** for server-state management
- Replace/rework manual API request handling where appropriate
- Add query caching, mutations, loading states, and error handling with TanStack Query

---

## Authentication

SprintBoard uses **JWT authentication with HttpOnly cookies**.

The JWT is issued when a user logs in or registers and is stored in an HttpOnly `access_token` cookie.

Using HttpOnly cookies means the JWT cannot be accessed directly through JavaScript, helping reduce the risk of token theft through client-side scripts.

### Authentication Routes

| Method | Route                       | Description                                  |
| ------ | --------------------------- | -------------------------------------------- |
| `POST` | `/api/Auth/register`        | Register a new user                          |
| `POST` | `/api/Auth/login`           | Log in with credentials                      |
| `GET`  | `/api/Auth/google`          | Start Google OAuth login                     |
| `GET`  | `/api/Auth/google/complete` | Complete Google OAuth login                  |
| `GET`  | `/api/Auth/me`              | Get the currently authenticated user         |
| `POST` | `/api/Auth/Logout`          | Log out and remove the authentication cookie |

### Authentication Flow

```text
Register/Login
      ↓
Frontend sends authentication request
      ↓
API validates credentials
      ↓
JWT generated
      ↓
JWT stored in HttpOnly cookie
      ↓
Authenticated requests
      ↓
JWT read from cookie
      ↓
User identity established
```

For Google authentication:

```text
Frontend
   ↓
/api/Auth/google
   ↓
Google OAuth
   ↓
/api/Auth/google/complete
   ↓
User created/found
   ↓
JWT generated
   ↓
JWT stored in HttpOnly cookie
   ↓
Frontend redirected to application
```

---

# Frontend

The frontend is being developed using React.

Authentication, the project list, and the project Kanban board are functional. At the moment, all API requests are handled using standard JavaScript/TypeScript `async/await`.

For example, the current approach for a request is conceptually:

```text
React component
      ↓
async/await API request
      ↓
ASP.NET Core API
      ↓
Global exception middleware (on failure)
      ↓
Response returned to frontend
```

### Kanban Board

The project board fetches a project's issues and members on load, then renders one column per issue status. Dragging a card to a different column updates the UI immediately and sends a `PUT` request to persist the new status, rolling back the local change if the request fails.

Issues can be created, edited, and deleted from a modal reachable from the board, including selecting an assignee from the project's member list.

### Planned Frontend State Management

The current API calls work without a dedicated server-state library. However, **TanStack Query** is planned for a later stage of development.

The intention is to use TanStack Query for things such as:

- Fetching the current user
- Fetching projects, project members, and issues
- Creating, updating, and deleting projects and issues
- Managing loading states
- Managing API errors
- Query caching
- Invalidating/refetching data after mutations

The initial implementation will continue using `async/await` until the frontend functionality is more established.

---

# Projects

Users are linked to projects through **Project Members**, allowing a project to have multiple users associated with it, each with a project role.

The current project relationship is conceptually:

```text
User
 │
 ├── Project Members
 │       │
 │       └── Project
 │
 └── Projects they belong to
```

This relationship is used for project-level permissions and collaboration.

### Current Project Routes

| Method   | Route                                             | Description                                |
| -------- | ------------------------------------------------- | ------------------------------------------ |
| `POST`   | `/api/Project/CreateProject`                      | Create a new project                       |
| `GET`    | `/api/Project/MyProjects`                         | Get projects belonging to the current user |
| `GET`    | `/api/Project/{id}`                               | Get a project by ID                        |
| `PUT`    | `/api/Project/{projectId}`                        | Update a project                           |
| `DELETE` | `/api/Project/{projectId}`                        | Delete a project                           |
| `GET`    | `/api/Project/{projectId}/members`                | Get a project's members                    |
| `POST`   | `/api/Project/{projectId}/members`                | Add a member to a project by email         |
| `DELETE` | `/api/Project/{projectId}/members/{targetUserId}` | Remove a member from a project             |

---

# Issues

Issues belong to a project and track work items through a status pipeline (`Todo`, `InProgress`, `InReview`, `Done`).

New issues are always created with a status of `Todo`; status is changed afterwards via update.

### Current Issue Routes

| Method   | Route                                        | Description                  |
| -------- | -------------------------------------------- | ---------------------------- |
| `GET`    | `/api/projects/{projectId}/issues`           | Get all issues for a project |
| `GET`    | `/api/projects/{projectId}/issues/{issueId}` | Get a single issue by ID     |
| `POST`   | `/api/projects/{projectId}/issues`           | Create a new issue           |
| `PUT`    | `/api/projects/{projectId}/issues/{issueId}` | Update an issue              |
| `DELETE` | `/api/projects/{projectId}/issues/{issueId}` | Delete an issue              |

Only users who are members of a project can view or modify its issues. Assigning an issue to a user requires that user to be a member of the same project.

---

# Users

User functionality is currently focused around authentication and retrieving user information.

### Current User Routes

| Method | Route            | Description           |
| ------ | ---------------- | --------------------- |
| `GET`  | `/api/User/{id}` | Retrieve a user by ID |

---

# Error Handling

The API uses a global exception handling middleware so that errors thrown from services are converted into consistent HTTP responses instead of leaking unhandled exceptions.

Known exception types (e.g. not-found and forbidden cases) are mapped to their corresponding HTTP status codes centrally, rather than being caught individually in each controller action.

---

# DTOs

The API uses DTOs to control the data being sent between the client and API rather than exposing database models directly.

### Authentication

- **`LoginDto`** — used when a user logs in
- **`RegisterDto`** — used when a user creates an account
- **`UserDto`** — used to return user information from the API
- **`UpdateUserDto`** — used to update a user's editable fields (name, avatar)

### Projects

- **`ProjectDto`** — used to create or update a project
- **`ProjectResponseDto`** — used to return project information from the API
- **`AddMemberDto`** — used to add a member to a project by email
- **`ProjectMemberDto`** — used to return a project member, including their role and join date

### Issues

- **`CreateIssueDto`** — used to create a new issue
- **`UpdateIssueDto`** — used to update an existing issue, including its status
- **`AssigneeDto`** — a lightweight representation of an issue's assignee
- **`IssueResponseDto`** — used to return issue information from the API

### Enums

- **`UserRole`** — defines the roles available to users within the application
- **`ProjectRole`** — defines a member's role within a specific project
- **`Priority`** — an issue's priority (`Low`, `Medium`, `High`)
- **`IssueStatus`** — an issue's status (`Todo`, `InProgress`, `InReview`, `Done`)

All enums are serialized as their string names (not numeric values) across the API.

---

# Current API Structure

```text
/api
│
├── /Auth
│   ├── POST   /register
│   ├── POST   /login
│   ├── GET    /google
│   ├── GET    /google/complete
│   ├── GET    /me
│   └── POST   /Logout
│
├── /Project
│   ├── POST   /CreateProject
│   ├── GET    /MyProjects
│   ├── GET    /{id}
│   ├── PUT    /{projectId}
│   ├── DELETE /{projectId}
│   ├── GET    /{projectId}/members
│   ├── POST   /{projectId}/members
│   └── DELETE /{projectId}/members/{targetUserId}
│
├── /projects/{projectId}/issues
│   ├── GET    /
│   ├── GET    /{issueId}
│   ├── POST   /
│   ├── PUT    /{issueId}
│   └── DELETE /{issueId}
│
└── /User
    └── GET    /{id}
```

---

# Data Model

---

# Technology

The backend is currently being developed using:

- **C#**
- **ASP.NET Core Web API**
- **Entity Framework Core**
- **JWT Authentication**
- **HttpOnly Cookies**
- **Google OAuth**
- **DTOs**
- **Global exception handling middleware**
- **REST API**

The frontend is being developed using:

- **React**
- **JavaScript/TypeScript**
- **Tailwind CSS**
- Standard `async/await` API requests
- **TanStack Query planned for later integration**
- **SignalR planned for realtime behaviour**

---

# Roadmap

## Authentication

- [x] User registration
- [x] User login
- [x] JWT authentication
- [x] HttpOnly authentication cookies
- [x] Google authentication
- [x] Current user endpoint
- [x] Logout
- [ ] Further authentication/authorization improvements

## Users

- [x] User model
- [x] User DTO
- [x] Get user by ID
- [x] User roles
- [ ] Project-specific permissions

## Projects

- [x] Project model
- [x] Project member relationship
- [x] Create project
- [x] Get user's projects
- [x] Get project by ID
- [x] Update project
- [x] Delete project
- [x] Add project members
- [x] Remove project members
- [x] Project permissions
- [x] Project roles

## Issues & Sprints

- [x] Issue model
- [x] Create issues
- [x] Update issues
- [x] Delete issues
- [x] Assign issues to users
- [x] Issue status
- [ ] Sprint model
- [ ] Create sprints
- [ ] Assign tasks to sprints
- [ ] Sprint progress

## Frontend

- [x] Initial React application
- [x] Login page
- [x] Login page connected to authentication API
- [x] Authentication using standard `async/await`
- [x] Registration page
- [x] Authentication state handling
- [x] Project list
- [x] Project Kanban board
- [x] Issue create/edit/delete modal
- [x] Assignee picker
- [ ] Project member management UI
- [ ] Sprint management UI
- [ ] Introduce TanStack Query
- [ ] Migrate suitable API requests to TanStack Query
- [ ] Add query caching and invalidation
- [ ] Add mutation handling

---

# Project Status

**SprintBoard is currently in the backend/API and frontend development stage.**

Authentication, Project management, and Issue management are in place, including a working Kanban board on the frontend with drag-and-drop status updates, issue CRUD, and an assignee picker backed by project membership.

The current focus is project member management on the frontend and building out Sprint functionality.

TanStack Query is planned for a later stage to improve server-state management as the number of API interactions grows.
