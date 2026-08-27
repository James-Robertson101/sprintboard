import ProjectNavigation from "../navigation/ProjectNavigation";
import { Outlet } from "react-router-dom";
import TopNav from "./TopNav";
import { useAuth } from "../../context/useAuth";
import Sidebar from "./Sidebar";

function ProjectLayout() {
  const { user } = useAuth();

  return (
    <div>
      <TopNav
        name={user?.name}
        email={user?.email}
        avatarUrl={user?.avatarUrl}
      />
      <div className="flex">
        <Sidebar />
        <div className="flex-1 min-w-0">
          <ProjectNavigation />
        </div>

        <Outlet />
      </div>
    </div>
  );
}

export default ProjectLayout;
