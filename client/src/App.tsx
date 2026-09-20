import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import "./App.css";
import { AuthProvider } from "./context/AuthProvider.tsx";
import ProtectedRoute from "./components/auth/ProtectedRoute";

import Login from "./pages/auth/Login.tsx";
import Register from "./pages/auth/Register.tsx";
import ProjectList from "./pages/projects/ProjectList.tsx";
import Sprints from "./pages/projects/Sprints/Sprints.tsx";
import ProjectLayout from "./components/layout/ProjectLayout.tsx";
import ProjectBoard from "./pages/projects/project/ProjectBoard.tsx";
import Backlog from "./pages/projects/project/Backlog.tsx";
import ProjectSettings from "./pages/projects/project/ProjectSettings/ProjectSettings.tsx";
import Profile from "./pages/profile/Profile.tsx";
import AppLayout from "./components/layout/AppLayout.tsx";
import Reports from "./pages/projects/project/Reports.tsx";
import { AuthGatedSignalR } from "./signalr/AuthGatedSignalR.tsx";

import CreateProject from "./pages/projects/CreateProject.tsx";
import { useEffect } from "react";
import { reseedIfDue } from "./services/systemService.ts";

function App() {
  useEffect(() => {
    reseedIfDue();
  }, []);

  return (
    <BrowserRouter>
      <AuthProvider>
        <AuthGatedSignalR>
          <Routes>
            {/* Authentication */}
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />

            {/* Everything below requires auth */}
            <Route element={<ProtectedRoute />}>
              <Route path="/" element={<Navigate to="/login" replace />} />
              <Route element={<AppLayout />}>
                <Route path="/projects" element={<ProjectList />} />
                <Route path="/profile" element={<Profile />} />
                <Route path="/projects/create" element={<CreateProject />} />
              </Route>

              <Route path="/projects/:projectId" element={<ProjectLayout />}>
                <Route index element={<Navigate to="board" replace />} />
                <Route path="board" element={<ProjectBoard />} />
                <Route path="backlog" element={<Backlog />} />
                <Route path="sprints" element={<Sprints />} />
                <Route path="reports" element={<Reports />} />
                <Route path="projectSettings" element={<ProjectSettings />} />
              </Route>
            </Route>
          </Routes>
        </AuthGatedSignalR>
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;
