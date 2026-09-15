import type {
  Comment,
  CreateCommentPayload,
  UpdateCommentPayload,
} from "../types/Comment";

const API_URL = import.meta.env.VITE_API_URL;

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || "Something went wrong.");
  }

  return response.json();
}

export async function getComments(
  projectId: string | number,
  issueId: number,
): Promise<Comment[]> {
  const response = await fetch(
    `${API_URL}/api/projects/${projectId}/issues/${issueId}/comments`,
    {
      credentials: "include",
    },
  );

  return handleResponse<Comment[]>(response);
}

export async function createComment(
  projectId: string | number,
  issueId: number,
  payload: CreateCommentPayload,
): Promise<Comment> {
  const response = await fetch(
    `${API_URL}/api/projects/${projectId}/issues/${issueId}/comments`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      credentials: "include",
      body: JSON.stringify(payload),
    },
  );

  return handleResponse<Comment>(response);
}

export async function updateComment(
  projectId: string | number,
  issueId: number,
  commentId: number,
  payload: UpdateCommentPayload,
): Promise<Comment> {
  const response = await fetch(
    `${API_URL}/api/projects/${projectId}/issues/${issueId}/comments/${commentId}`,
    {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      credentials: "include",
      body: JSON.stringify(payload),
    },
  );

  return handleResponse<Comment>(response);
}

export async function deleteComment(
  projectId: string | number,
  issueId: number,
  commentId: number,
): Promise<void> {
  const response = await fetch(
    `${API_URL}/api/projects/${projectId}/issues/${issueId}/comments/${commentId}`,
    {
      method: "DELETE",
      credentials: "include",
    },
  );

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || "Failed to delete comment.");
  }
}
