import { createContext } from "react";
import * as signalR from "@microsoft/signalr";

// seperate file for the context object
// mirrors the auth context pattern this keeps fast refresh happy
export interface SignalRContextValue {
  connection: signalR.HubConnection | null;
  isConnected: boolean;
}

export const SignalRContext = createContext<SignalRContextValue>({
  connection: null,
  isConnected: false,
});
