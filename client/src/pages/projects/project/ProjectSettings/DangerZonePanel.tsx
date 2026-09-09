import { useNavigate } from "react-router-dom";
import { deleteProject } from "../../../../services/projectService";

interface DangerZonePanelProps {
  projectId: string;
}

export function DangerZonePanel({ projectId }: DangerZonePanelProps) {
  const navigate = useNavigate();

  async function handleDeleteProject() {
    const confirmed = window.confirm(
      "Are you sure you want to delete this project",
    );
    if (!confirmed) return;

    try {
      await deleteProject(Number(projectId));
      navigate("/projects");
    } catch (error) {
      // Consider surfacing this error in the UI rather than swallowing it
      console.error(error);
    }
  }

  return (
    <div className="rounded-xl border border-rose-200 bg-white shadow-sm">
      <div className="border-b border-rose-100 px-6 py-5">
        <h2 className="text-base font-semibold text-rose-700">Danger zone</h2>
        <p className="mt-1 text-sm text-slate-500">
          These actions can permanently affect your project.
        </p>
      </div>

      <div className="flex flex-col gap-4 px-6 py-6 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h3 className="text-sm font-semibold text-slate-900">
            Delete this project
          </h3>
          <p className="mt-1 max-w-xl text-sm text-slate-500">
            Permanently delete this project and its issues. This action cannot
            be undone.
          </p>
        </div>

        <button
          onClick={handleDeleteProject}
          type="button"
          className="shrink-0 rounded-lg border border-rose-300 px-4 py-2.5 text-sm font-semibold text-rose-600 transition hover:bg-rose-50"
        >
          Delete project
        </button>
      </div>
    </div>
  );
}
