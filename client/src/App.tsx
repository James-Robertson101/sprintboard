import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import "./App.css";
import { AuthProvider } from "./context/AuthProvider.tsx";
import ProtectedRoute from "./components/auth/ProtectedRoute";

import Login from "./pages/Login.tsx";
import Register from "./pages/Register.tsx";
import ProjectList from "./pages/ProjectList.tsx";

import ProjectLayout from "./components/layout/ProjectLayout.tsx";
import ProjectBoard from "./pages/CurrentProject.tsx";
import ProjectSettings from "./pages/ProjectSettings.tsx";
import CreateProject from "./pages/CreateProject.tsx";

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
              <Route index element={<Navigate to="all" replace />} />
              <Route path="all" element={<ProjectBoard />} />
              <Route path="started" element={<ProjectBoard />} />
              <Route path="approval" element={<ProjectBoard />} />
              <Route path="discrepancy" element={<ProjectBoard />} />
              <Route path="completed" element={<ProjectBoard />} />
              <Route path="settings" element={<ProjectSettings />} />
            </Route>
          </Route>
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;
