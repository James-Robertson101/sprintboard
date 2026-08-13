import ProjectHeader from "../ProjectHeader";
import ProjectNavigation from "../ProjectNavigation";
import { Outlet } from "react-router-dom";

function ProjectLayout() {
  return (
    <div>
      <ProjectHeader />

      <ProjectNavigation />

      <Outlet />
    </div>
  );
}

export default ProjectLayout;
