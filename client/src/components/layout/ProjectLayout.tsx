import ProjectNavigation from "../navigation/ProjectNavigation";
import { Outlet } from "react-router-dom";
import TopNav from "./TopNav";
import { useAuth } from "../../context/useAuth";
import Sidebar from "./Sidebar";

function ProjectLayout() {
  const { user } = useAuth();

  return (
    <div className="min-h-screen bg-slate-50">
      <TopNav
        name={user?.name}
        email={user?.email}
        avatarUrl={user?.avatarUrl}
      />
      <div className="flex">
        <Sidebar />

        <div className="flex min-w-0 flex-1 flex-col">
          <ProjectNavigation />
          <Outlet />
        </div>
      </div>
    </div>
  );
}

export default ProjectLayout;
