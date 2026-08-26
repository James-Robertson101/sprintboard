import type { Project } from "../types/project";

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
