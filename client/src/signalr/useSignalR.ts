import { useContext } from "react";
import { SignalRContext } from "./SignalRContext";

export function useSignalR() {
  return useContext(SignalRContext);
}
