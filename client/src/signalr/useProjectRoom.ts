import { useEffect, useRef } from "react";
import { useSignalR } from "./useSignalR";

export function useProjectRoom(projectId: string | undefined) {
  const { connection, isConnected } = useSignalR();
  const joinedProjectRef = useRef<string | null>(null);

  useEffect(() => {
    if (!connection || !isConnected || !projectId) return;

    const currentConnection = connection;
    const currentProjectId = projectId;

    async function joinProject() {
      try {
        await currentConnection.invoke("JoinProject", currentProjectId);

        joinedProjectRef.current = currentProjectId;

        console.log("[SignalR] Successfully joined project:", currentProjectId);
      } catch (error) {
        console.error(
          "[SignalR] Failed to join project:",
          currentProjectId,
          error,
        );
      }
    }

    joinProject();

    return () => {
      currentConnection
        .invoke("LeaveProject", currentProjectId)
        .catch(console.error);

      joinedProjectRef.current = null;
    };
  }, [connection, isConnected, projectId]);
}
