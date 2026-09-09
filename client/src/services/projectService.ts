import type {
  Project,
  ProjectData,
  ProjectMember,
  ProjectMemberRole,
  UserSummary,
} from "../types/project";
export async function getProjects(search?: string): Promise<Project[]> {
  const params = new URLSearchParams();

  if (search?.trim()) {
    params.append("search", search.trim());
  }

  const response = await fetch(
    `${import.meta.env.VITE_API_URL}/api/project/MyProjects?${params.toString()}`,
    {
      method: "GET",
      credentials: "include",
    },
  );

  if (!response.ok) {
    throw new Error("Failed to fetch projects");
  }

  return response.json();
}

export async function removeProjectMember(
  projectId: number,
  userId: number,
): Promise<void> {
  const response = await fetch(
    `${import.meta.env.VITE_API_URL}/api/project/${projectId}/members/${userId}`,
    {
      method: "DELETE",
      credentials: "include",
    },
  );

  if (!response.ok) {
    let message = "Failed to delete member";
    try {
      const result = await response.json();
      message = result.error || message;
    } catch {
      // body might be empty (e.g. plain 403/404 with no JSON) — ignore
    }
    throw new Error(message);
  }
}

export async function getProjectById(id: number): Promise<Project> {
  const response = await fetch(
    `${import.meta.env.VITE_API_URL}/api/project/${id}`,
    {
      method: "GET",
      credentials: "include",
    },
  );

  if (!response.ok) {
    throw new Error("Failed to fetch projects");
  }

  return response.json();
}

export async function createProject(projectData: ProjectData) {
  const response = await fetch(
    `${import.meta.env.VITE_API_URL}/api/project/createProject`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      credentials: "include",
      body: JSON.stringify(projectData),
    },
  );

  if (!response.ok) {
    throw new Error("Create Project Failed");
  }
}

export async function deleteProject(projectId: number): Promise<void> {
  const response = await fetch(
    `${import.meta.env.VITE_API_URL}/api/project/${projectId}`,
    {
      method: "DELETE",
      credentials: "include",
    },
  );

  if (!response.ok) {
    let message = "Failed to delete project";
    try {
      const result = await response.json();
      message = result.error || message;
    } catch {
      // body might be empty (e.g. plain 403/404 with no JSON) — ignore
    }
    throw new Error(message);
  }
}

interface ProjectMemberDto {
  userId: number;
  name: string;
  email: string;
  avatarUrl?: string | null;
  projectRole: string;
  joinTime: string;
}

export async function getProjectMembers(
  projectId: number | string,
): Promise<ProjectMember[]> {
  const response = await fetch(
    `${import.meta.env.VITE_API_URL}/api/project/${projectId}/members`,
    {
      method: "GET",
      credentials: "include",
    },
  );

  if (!response.ok) {
    throw new Error("Failed to fetch project members");
  }
  const members: ProjectMemberDto[] = await response.json();

  return members.map((member) => ({
    id: member.userId,
    name: member.name,
    email: member.email,
    avatarUrl: member.avatarUrl,
    role: member.projectRole as ProjectMemberRole,
  }));
}

export async function getAvailableUsers(projectId: string, search: string) {
  const res = await fetch(
    `${import.meta.env.VITE_API_URL}/api/project/${projectId}/available-users?search=${encodeURIComponent(search)}`,
    { credentials: "include" },
  );
  if (!res.ok) throw new Error("Failed to search users.");
  return res.json() as Promise<UserSummary[]>;
}

export async function addProjectMember(projectId: string, userId: number) {
  const res = await fetch(
    `${import.meta.env.VITE_API_URL}/api/project/${projectId}/members`,
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      credentials: "include",
      body: JSON.stringify({ userId }),
    },
  );
  if (!res.ok) throw new Error("Failed to add member.");
  return res.json() as Promise<ProjectMember>;
}

export async function updateProject(
  projectId: number,
  projectData: ProjectData,
): Promise<Project> {
  const response = await fetch(
    `${import.meta.env.VITE_API_URL}/api/project/${projectId}`,
    {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      credentials: "include",
      body: JSON.stringify(projectData),
    },
  );

  if (!response.ok) {
    let message = "Failed to update project";
    try {
      const result = await response.json();
      message = result.error || message;
    } catch {
      // body might be empty (e.g. plain 403/404 with no JSON) — ignore
    }
    throw new Error(message);
  }

  return response.json();
}
