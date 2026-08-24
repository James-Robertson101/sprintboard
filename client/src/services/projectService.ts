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
