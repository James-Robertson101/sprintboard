import * as signalR from "@microsoft/signalr";

//Holds one hub connection instance for the whole application
//requires auth credentials
let connection: signalR.HubConnection | null = null;

export function getConnection(): signalR.HubConnection {
  if (!connection) {
    connection = new signalR.HubConnectionBuilder()
      .withUrl(`${import.meta.env.VITE_API_URL}/hub/sprintboard`, {
        withCredentials: true,
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();
  }
  return connection;
}

export async function stopSignalRConnection() {
  if (connection) {
    await connection.stop();
    connection = null; // force a fresh instance on next login
  }
}
