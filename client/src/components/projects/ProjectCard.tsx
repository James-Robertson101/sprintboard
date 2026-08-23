import type { Project } from "../../types/project";
function ProjectCard({ name, description }: Project) {
  return (
    <div className="rounded-xl border bg-white p-6">
      <h2>{name}</h2>
      <p>{description}</p>
      <div className="mt-4">{/* stats */}</div>
    </div>
  );
}

export default ProjectCard;
