import ProjectNavigation from "../ProjectNavigation";
import { Outlet } from "react-router-dom";
import TopNav from "./TopNav";
import { useAuth } from "../../context/useAuth";

function ProjectLayout() {
  const { user } = useAuth();

  return (
    <div>
      <TopNav
        name={user?.name}
        email={user?.email}
        avatarUrl={user?.avatarUrl}
      />

      <ProjectNavigation />

      <Outlet />
    </div>
  );
}

export default ProjectLayout;
