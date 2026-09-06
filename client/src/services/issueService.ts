import type {
  Issue,
  CreateIssuePayload,
  UpdateIssuePayload,
} from "../types/Issue";

// Swap this for however your other services resolve the API base
// (e.g. an axios instance) if that differs from a bare env var.
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

export async function getIssues(projectId: string | number): Promise<Issue[]> {
  const res = await fetch(`${API_BASE}/api/projects/${projectId}/issues`, {
    credentials: "include",
  });
  return handleResponse<Issue[]>(res);
}

export async function getIssueById(
  projectId: string | number,
  issueId: number,
): Promise<Issue> {
  const res = await fetch(
    `${API_BASE}/api/projects/${projectId}/issues/${issueId}`,
    { credentials: "include" },
  );
  return handleResponse<Issue>(res);
}

export async function createIssue(
  projectId: string | number,
  payload: CreateIssuePayload,
): Promise<Issue> {
  const res = await fetch(`${API_BASE}/api/projects/${projectId}/issues`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(payload),
  });
  return handleResponse<Issue>(res);
}

export async function updateIssue(
  projectId: string | number,
  issueId: number,
  payload: UpdateIssuePayload,
): Promise<Issue> {
  const res = await fetch(
    `${API_BASE}/api/projects/${projectId}/issues/${issueId}`,
    {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      credentials: "include",
      body: JSON.stringify(payload),
    },
  );
  return handleResponse<Issue>(res);
}

export async function deleteIssue(
  projectId: string | number,
  issueId: number,
): Promise<void> {
  const res = await fetch(
    `${API_BASE}/api/projects/${projectId}/issues/${issueId}`,
    {
      method: "DELETE",
      credentials: "include",
    },
  );
  return handleResponse<void>(res);
}
