import type { RegisterData, LoginData } from "../types/auth";

export async function registerUser(data: RegisterData) {
  const response = await fetch(
    `${import.meta.env.VITE_API_URL}/api/auth/register`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      credentials: "include",
      body: JSON.stringify(data),
    },
  );

  const result = await response.json();

  if (!response.ok) {
    throw new Error(result.error || "Registration failed");
  }

  return result;
}

export async function loginUser(data: LoginData) {
  const response = await fetch(
    `${import.meta.env.VITE_API_URL}/api/auth/login`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      credentials: "include",
      body: JSON.stringify(data),
    },
  );

  const result = await response.json();

  if (!response.ok) {
    throw new Error(result.error || "Login failed");
  }

  return result;
}

export function handleGoogleLogin() {
  const apiUrl = import.meta.env.VITE_API_URL;

  if (!apiUrl) {
    console.error("VITE_API_URL is not configured.");
    throw new Error("Unable to connect to the server.");
  }

  window.location.href = `${apiUrl}/api/auth/google`;
}
