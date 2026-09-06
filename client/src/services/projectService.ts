import type {
  Project,
  ProjectData,
  ProjectMember,
  ProjectMemberRole,
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

// Mirrors ProjectMemberDto (UserId, Name, AvatarUrl, ProjectRole, JoinTime).
// Only the fields the assignee picker needs are pulled out below; add
// projectRole/joinTime here too if you need them elsewhere later.
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
