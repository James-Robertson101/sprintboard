import type { RegisterData } from "../types/auth";
import type { LoginData } from "../types/auth";

export async function registerUser(data: RegisterData) {
  const response = await fetch(
    `${import.meta.env.VITE_API_URL}/api/auth/register`,
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    },
  );

  const result = await response.json();

  if (!response.ok) {
    throw new Error(result.message || "Registration failed");
  }
  return result;
}

export async function loginUser(data: LoginData) {
  const response = await fetch(
    `${import.meta.env.VITE_API_URL}/api/auth/login`,
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      credentials: "include",
      body: JSON.stringify(data),
    },
  );

  const result = await response.json();

  if (!response.ok) {
    throw new Error(result.message || "Registration failed");
  }
  return result;
}
