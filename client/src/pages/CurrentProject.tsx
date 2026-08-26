import { useParams } from "react-router-dom";
function CurrentProject() {
  const { projectId } = useParams();
  console.log(projectId);

  return <></>;
}

export default CurrentProject;
