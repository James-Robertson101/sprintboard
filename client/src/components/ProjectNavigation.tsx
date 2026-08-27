import { Link, useParams } from "react-router-dom";

function ProjectNavigation() {
  const { projectId } = useParams();

  return (
    <div className="flex w-full justify-between p-3">
      <div className="flex gap-6">
        <Link className="font-bold" to={`/projects/${projectId}/board`}>
          Board
        </Link>
        <Link className="font-bold" to={`/projects/${projectId}/backlog`}>
          Backlog
        </Link>
        <Link className="font-bold" to={`/projects/${projectId}/issues`}>
          Issues
        </Link>
        <Link className="font-bold" to={`/projects/${projectId}/sprints`}>
          Sprints
        </Link>
        <Link className="font-bold" to={`/projects/${projectId}/reports`}>
          Reports
        </Link>
        <Link
          className="font-bold"
          to={`/projects/${projectId}/projectSettings`}
        >
          Project Settings
        </Link>
      </div>
    </div>
  );
}

export default ProjectNavigation;
