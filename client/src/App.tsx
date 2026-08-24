import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import "./App.css";

import Login from "./pages/Login.tsx";
import Register from "./pages/Register.tsx";
import ProjectList from "./pages/ProjectList.tsx";

import ProjectLayout from "./components/layout/ProjectLayout.tsx";
import ProjectBoard from "./pages/CurrentProject.tsx";
import ProjectSettings from "./pages/ProjectSettings.tsx";

function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Authentication */}
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />

        {/* Project list */}
        <Route path="/" element={<Navigate to="/login" />} />
        <Route path="/projects" element={<ProjectList />} />

        {/* Individual project */}
        <Route path="/projects/:projectId" element={<ProjectLayout />}>
          {/* Default project route */}
          <Route index element={<Navigate to="all" replace />} />

          {/* Project status views */}
          <Route path="all" element={<ProjectBoard />} />
          <Route path="started" element={<ProjectBoard />} />
          <Route path="approval" element={<ProjectBoard />} />
          <Route path="discrepancy" element={<ProjectBoard />} />
          <Route path="completed" element={<ProjectBoard />} />

          {/* Project settings */}
          <Route path="settings" element={<ProjectSettings />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
