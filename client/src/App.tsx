import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import "./App.css";
import { AuthProvider } from "./context/AuthProvider.tsx";
import ProtectedRoute from "./components/auth/ProtectedRoute";

import Login from "./pages/auth/Login.tsx";
import Register from "./pages/auth/Register.tsx";
import ProjectList from "./pages/projects/ProjectList.tsx";

import ProjectLayout from "./components/layout/ProjectLayout.tsx";
import ProjectBoard from "./pages/projects/project/ProjectBoard.tsx";
import ProjectSettings from "./pages/projects/project/ProjectSettings/ProjectSettings.tsx";
import CreateProject from "./pages/projects/CreateProject.tsx";

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          {/* Authentication */}
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />

          {/* Everything below requires auth */}
          <Route element={<ProtectedRoute />}>
            <Route path="/" element={<Navigate to="/login" replace />} />
            <Route path="/projects" element={<ProjectList />} />
            <Route path="/projects/create" element={<CreateProject />} />

            <Route path="/projects/:projectId" element={<ProjectLayout />}>
              <Route index element={<Navigate to="board" replace />} />
              <Route path="board" element={<ProjectBoard />} />
              <Route path="backlog" element={<ProjectBoard />} />
              <Route path="issues" element={<ProjectBoard />} />
              <Route path="sprints" element={<ProjectBoard />} />
              <Route path="reports" element={<ProjectBoard />} />
              <Route path="projectSettings" element={<ProjectSettings />} />
            </Route>
          </Route>
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;
