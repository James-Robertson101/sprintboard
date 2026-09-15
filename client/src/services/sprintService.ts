import type {
  Sprint,
  CreateSprintPayload,
  UpdateSprintPayload,
} from "../types/Sprint";

const API_URL = import.meta.env.VITE_API_URL;

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || "Something went wrong.");
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json();
}

export async function getSprints(
  projectId: string | number,
): Promise<Sprint[]> {
  const response = await fetch(`${API_URL}/api/projects/${projectId}/sprints`, {
    credentials: "include",
  });

  return handleResponse<Sprint[]>(response);
}

export async function getSprint(
  projectId: string | number,
  sprintId: number,
): Promise<Sprint> {
  const response = await fetch(
    `${API_URL}/api/projects/${projectId}/sprints/${sprintId}`,
    {
      credentials: "include",
    },
  );

  return handleResponse<Sprint>(response);
}

export async function createSprint(
  projectId: string | number,
  payload: CreateSprintPayload,
): Promise<Sprint> {
  const response = await fetch(`${API_URL}/api/projects/${projectId}/sprints`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    credentials: "include",
    body: JSON.stringify(payload),
  });

  return handleResponse<Sprint>(response);
}

export async function updateSprint(
  projectId: string | number,
  sprintId: number,
  payload: UpdateSprintPayload,
): Promise<Sprint> {
  const response = await fetch(
    `${API_URL}/api/projects/${projectId}/sprints/${sprintId}`,
    {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      credentials: "include",
      body: JSON.stringify(payload),
    },
  );

  return handleResponse<Sprint>(response);
}

export async function startSprint(
  projectId: string | number,
  sprintId: number,
): Promise<Sprint> {
  const response = await fetch(
    `${API_URL}/api/projects/${projectId}/sprints/${sprintId}/start`,
    {
      method: "POST",
      credentials: "include",
    },
  );

  return handleResponse<Sprint>(response);
}

export async function completeSprint(
  projectId: string | number,
  sprintId: number,
): Promise<Sprint> {
  const response = await fetch(
    `${API_URL}/api/projects/${projectId}/sprints/${sprintId}/complete`,
    {
      method: "POST",
      credentials: "include",
    },
  );

  return handleResponse<Sprint>(response);
}

export async function deleteSprint(
  projectId: string | number,
  sprintId: number,
): Promise<void> {
  const response = await fetch(
    `${API_URL}/api/projects/${projectId}/sprints/${sprintId}`,
    {
      method: "DELETE",
      credentials: "include",
    },
  );

  await handleResponse<void>(response);
}
