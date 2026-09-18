import { useEffect, useRef, useState, type ReactNode } from "react";
import { getConnection } from "./connection";
import { SignalRContext } from "./SignalRContext";

export function SignalRProvider({
  isAuthenticated,
  children,
}: {
  isAuthenticated: boolean;
  children: ReactNode;
}) {
  const [isConnected, setIsConnected] = useState(false);
  const startedRef = useRef(false);

  useEffect(() => {
    if (!isAuthenticated || startedRef.current) return;

    const conn = getConnection();
    startedRef.current = true;

    conn.onreconnected(() => setIsConnected(true));
    conn.onreconnecting(() => setIsConnected(false));
    conn.onclose(() => setIsConnected(false));

    conn
      .start()
      .then(() => setIsConnected(true))
      .catch((err) => console.error("SignalR connection failed:", err));

    return () => {
      // Only resets the local "have we started" flag so a future login can
      // start fresh. Does NOT call setState here — actual disconnect state
      // is driven by conn.onclose above, which fires when stopSignalRConnection()
      // runs from logout().
      startedRef.current = false;
    };
  }, [isAuthenticated]);

  return (
    <SignalRContext.Provider
      value={{ connection: isConnected ? getConnection() : null, isConnected }}
    >
      {children}
    </SignalRContext.Provider>
  );
}
