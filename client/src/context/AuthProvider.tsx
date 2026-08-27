import { useEffect, useState, type ReactNode } from "react";
import { AuthContext } from "./AuthContext";
import { getUser, logout as logoutRequest } from "../services/authService";
import type { User } from "../types/auth";

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  async function refetchUser() {
    try {
      const userData = await getUser();
      setUser(userData);
    } catch (error) {
      console.log("error", error);
      setUser(null);
    } finally {
      setLoading(false);
    }
  }

  async function logout() {
    try {
      await logoutRequest();
    } finally {
      setUser(null);
    }
  }

  useEffect(() => {
    let ignore = false;

    async function loadInitialUser() {
      try {
        const userData = await getUser();

        if (!ignore) {
          setUser(userData);
        }
      } catch (error) {
        console.log("error", error);

        if (!ignore) {
          setUser(null);
        }
      } finally {
        if (!ignore) {
          setLoading(false);
        }
      }
    }

    loadInitialUser();

    return () => {
      ignore = true;
    };
  }, []);

  return (
    <AuthContext.Provider value={{ user, loading, refetchUser, logout }}>
      {children}
    </AuthContext.Provider>
  );
}
