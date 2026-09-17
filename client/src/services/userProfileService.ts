import type { User } from "../types/auth";

const API_URL = import.meta.env.VITE_API_URL;

export interface UpdateProfilePayload {
  name: string;
  avatarUrl?: string | null;
}

// Deviation from the comment service's handleResponse: the backend returns
// JSON error bodies here (e.g. { message: "..." } on 409 conflicts), not
// plain text, so we try JSON first and fall back to text/status if that fails.
async function parseErrorMessage(
  response: Response,
  fallback: string,
): Promise<string> {
  const raw = await response.text();

  if (!raw) return fallback;

  try {
    const parsed = JSON.parse(raw) as { message?: string };
    return parsed.message || fallback;
  } catch {
    return raw;
  }
}

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    throw new Error(await parseErrorMessage(response, "Something went wrong."));
  }

  return response.json();
}

export async function updateProfile(
  payload: UpdateProfilePayload,
): Promise<User> {
  const response = await fetch(`${API_URL}/api/auth/me`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    credentials: "include",
    body: JSON.stringify(payload),
  });

  return handleResponse<User>(response);
}

export async function deleteMyAccount(): Promise<void> {
  const response = await fetch(`${API_URL}/api/auth/me`, {
    method: "DELETE",
    credentials: "include",
  });

  if (!response.ok) {
    throw new Error(
      await parseErrorMessage(response, "Failed to delete account."),
    );
  }
}
