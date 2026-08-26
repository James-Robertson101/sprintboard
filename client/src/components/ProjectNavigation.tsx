import { Link, useParams } from "react-router-dom";

function ProjectNavigation() {
  const { projectId } = useParams();

  return (
    <div className="flex w-full justify-between">
      <div className="flex gap-6">
        <Link to={`/projects/${projectId}/all`}>All</Link>
        <Link to={`/projects/${projectId}/started`}>Started</Link>
        <Link to={`/projects/${projectId}/approval`}>Approval</Link>
        <Link to={`/projects/${projectId}/discrepancy`}>Discrepancy</Link>
        <Link to={`/projects/${projectId}/completed`}>Completed</Link>
      </div>
    </div>
  );
}

export default ProjectNavigation;
