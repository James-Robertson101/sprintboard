import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import "./App.css";
import Login from "./pages/Login.tsx";
import Register from "./pages/Register.tsx";
import ProjectLayout from "./components/layout/ProjectLayout.tsx";
import ProjectBoard from "./pages/ProjectBoard.tsx";
import ProjectList from "./pages/ProjectList.tsx";
import ProjectSettings from "./pages/ProjectSettings.tsx";

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Navigate to="/login" />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/projectList" element={<ProjectList />} />
        <Route path="/projects/:projectId" element={<ProjectLayout />}>
          <Route path="board" element={<ProjectBoard />} />
          <Route path="settings" element={<ProjectSettings />} />
        </Route>{" "}
        #this will need to be projects/:projectId
      </Routes>
    </BrowserRouter>
  );
}

export default App;
