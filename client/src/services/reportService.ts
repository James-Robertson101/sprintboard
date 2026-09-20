import type {
  BurndownPoint,
  SprintListItem,
  SprintSummary,
  VelocityPoint,
} from "../types/Report";

const API_BASE = import.meta.env.VITE_API_URL ?? "";

async function handleResponse<T>(res: Response): Promise<T> {
  if (!res.ok) {
    const message = await res.text().catch(() => "");
    throw new Error(message || `Request failed with status ${res.status}`);
  }
  if (res.status === 204) {
    return undefined as T;
  }
  return (await res.json()) as T;
}

export async function getSprintSummary(
  projectId: string | number,
  sprintId: number,
): Promise<SprintSummary> {
  const res = await fetch(
    `${API_BASE}/api/projects/${projectId}/reports/sprints/${sprintId}/summary`,
    { credentials: "include" },
  );
  return handleResponse<SprintSummary>(res);
}

export async function getBurndown(
  projectId: string | number,
  sprintId: number,
): Promise<BurndownPoint[]> {
  const res = await fetch(
    `${API_BASE}/api/projects/${projectId}/reports/sprints/${sprintId}/burndown`,
    { credentials: "include" },
  );
  return handleResponse<BurndownPoint[]>(res);
}

export async function getVelocity(
  projectId: string | number,
  sprints = 6,
): Promise<VelocityPoint[]> {
  const res = await fetch(
    `${API_BASE}/api/projects/${projectId}/reports/velocity?sprints=${sprints}`,
    { credentials: "include" },
  );
  return handleResponse<VelocityPoint[]>(res);
}

// NOTE: if you already have a "get sprints" function in your sprint service,
// delete this one and import that instead. This assumes the route
// GET /api/projects/{projectId}/sprints returns SprintResponseDto[].
export async function getProjectSprints(
  projectId: string | number,
): Promise<SprintListItem[]> {
  const res = await fetch(`${API_BASE}/api/projects/${projectId}/sprints`, {
    credentials: "include",
  });
  return handleResponse<SprintListItem[]>(res);
}
