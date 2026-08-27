import { useParams } from "react-router-dom";
function ProjectBoard() {
  const { projectId } = useParams();
  console.log(projectId);

  return <></>;
}

export default ProjectBoard;
