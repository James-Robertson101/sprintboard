# SprintBoard

SprintBoard is a project and task management application currently under development.

The goal of SprintBoard is to allow users to create and manage projects, work with other project members, and eventually manage tasks and sprints within those projects.

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
- Basic project creation
- Retrieving a user's projects
- Retrieving a project by ID
- Retrieving a user by ID
- DTO-based API structure
- User roles
- React frontend login page
- Frontend login page connected to the authentication API
- Authentication requests currently handled using standard `async/await`

### Currently working on

- Completing project methods
- Project member management
- Project authorization/permissions
- Further project functionality
- Task management
- Sprint functionality

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

The login page has already been built and connected to the authentication API. At the moment, authentication requests are handled using standard JavaScript/TypeScript `async/await`.

For example, the current approach is conceptually:

```text
React Login Page
      ↓
async/await API request
      ↓
POST /api/Auth/login
      ↓
ASP.NET Core API
      ↓
JWT created
      ↓
HttpOnly cookie
      ↓
Authenticated frontend
```

### Planned Frontend State Management

The current API calls work without a dedicated server-state library. However, **TanStack Query** is planned for a later stage of development.

The intention is to use TanStack Query for things such as:

- Fetching the current user
- Fetching projects
- Fetching individual projects
- Creating projects
- Updating projects
- Deleting projects
- Managing loading states
- Managing API errors
- Query caching
- Invalidating/refetching project data after mutations

The initial implementation will continue using `async/await` until the frontend functionality is more established.

---

# Projects

Project functionality is currently being developed.

Users are linked to projects through **Project Members**, allowing a project to have multiple users associated with it.

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

This relationship will eventually be used for project-level permissions and collaboration.

### Current Project Routes

| Method | Route                        | Description                                |
| ------ | ---------------------------- | ------------------------------------------ |
| `POST` | `/api/Project/CreateProject` | Create a new project                       |
| `GET`  | `/api/Project/MyProjects`    | Get projects belonging to the current user |
| `GET`  | `/api/Project/{id}`          | Get a project by ID                        |

More project functionality is currently being implemented.

---

# Users

User functionality is currently focused around authentication and retrieving user information.

### Current User Routes

| Method | Route            | Description           |
| ------ | ---------------- | --------------------- |
| `GET`  | `/api/User/{id}` | Retrieve a user by ID |

---

# DTOs

The API uses DTOs to control the data being sent between the client and API rather than exposing database models directly.

Current DTOs include:

### `LoginDto`

Used when a user logs in.

### `RegisterDto`

Used when a user creates an account.

### `UserDto`

Used to return user information from the API.

### `ProjectDto`

Used to return project information from the API.

### `UserRole`

Defines the roles available to users within the application.

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
│   └── GET    /{id}
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
- **REST API**

The frontend is being developed using:

- **React**
- **JavaScript/TypeScript**
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
- [ ] Update project
- [ ] Delete project
- [ ] Add project members
- [ ] Remove project members
- [ ] Project permissions
- [ ] Project roles

## Tasks & Sprints

- [ ] Task model
- [ ] Create tasks
- [ ] Update tasks
- [ ] Delete tasks
- [ ] Assign tasks to users
- [ ] Task status
- [ ] Sprint model
- [ ] Create sprints
- [ ] Assign tasks to sprints
- [ ] Sprint progress

## Frontend

- [x] Initial React application
- [x] Login page
- [x] Login page connected to authentication API
- [x] Authentication using standard `async/await`
- [ ] Registration page
- [ ] Authentication state handling
- [ ] Project list
- [ ] Project dashboard
- [ ] Project member management
- [ ] Task management UI
- [ ] Sprint management UI
- [ ] Introduce TanStack Query
- [ ] Migrate suitable API requests to TanStack Query
- [ ] Add query caching and invalidation
- [ ] Add mutation handling

---

# Project Status

**SprintBoard is currently in the backend/API and frontend development stage.**

Authentication and the initial User/Project architecture are in place. The React login page is connected to the authentication API and is currently using standard `async/await` requests.

The current focus is expanding the Project functionality and building the foundation required for project collaboration, tasks, and sprints.

TanStack Query is planned for a later stage to improve server-state management as the number of API interactions grows.
