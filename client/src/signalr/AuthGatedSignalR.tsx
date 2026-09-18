import type { ReactNode } from "react";
import { useAuth } from "../context/useAuth";
import { SignalRProvider } from "./SignalRProvider";

export function AuthGatedSignalR({ children }: { children: ReactNode }) {
  const { user, loading } = useAuth();

  if (loading) return <>{children}</>;

  return <SignalRProvider isAuthenticated={!!user}>{children}</SignalRProvider>;
}
