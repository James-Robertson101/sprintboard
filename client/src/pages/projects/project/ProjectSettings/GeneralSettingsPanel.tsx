import { useEffect, useState } from "react";
import {
  getProjectById,
  updateProject,
} from "../../../../services/projectService";
import AvatarModal from "../../../../components/auth/AvatarModal";

interface GeneralSettingsPanelProps {
  projectId: string;
}

export function GeneralSettingsPanel({ projectId }: GeneralSettingsPanelProps) {
  const [projectName, setProjectName] = useState("");
  const [description, setDescription] = useState("");
  const [icon, setIcon] = useState("");
  const [iconModalOpen, setIconModalOpen] = useState(false);

  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function loadProject() {
      setIsLoading(true);
      setError(null);

      try {
        const project = await getProjectById(Number(projectId));
        setProjectName(project.name);
        setDescription(project.description ?? "");
        setIcon(project.icon ?? "");
      } catch (err) {
        setError(
          err instanceof Error ? err.message : "Failed to load project.",
        );
      } finally {
        setIsLoading(false);
      }
    }

    loadProject();
  }, [projectId]);

  async function handleUpdateProject(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setIsSaving(true);
    setError(null);

    try {
      await updateProject(Number(projectId), {
        name: projectName,
        description,
        icon,
      });
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to update project.",
      );
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <>
      <form
        onSubmit={handleUpdateProject}
        className="rounded-xl border border-slate-200 bg-white shadow-sm"
      >
        <div className="border-b border-slate-200 px-6 py-5">
          <h2 className="text-base font-semibold text-slate-900">General</h2>
          <p className="mt-1 text-sm text-slate-500">
            Update the basic information for this project.
          </p>
        </div>

        {error && (
          <div className="mx-6 mt-5 rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
            {error}
          </div>
        )}

        <div className="space-y-6 px-6 py-6">
          {/* Icon */}
          <div>
            <label className="mb-2 block text-sm font-medium text-slate-700">
              Project icon
            </label>

            <div className="flex items-center gap-4">
              {icon ? (
                <img
                  src={icon}
                  alt="Project icon"
                  className="h-16 w-16 rounded-full border border-slate-200 object-cover"
                />
              ) : (
                <div className="flex h-16 w-16 items-center justify-center rounded-full border border-dashed border-slate-300 bg-slate-50 text-xs text-slate-400">
                  None
                </div>
              )}

              <button
                type="button"
                onClick={() => setIconModalOpen(true)}
                disabled={isLoading}
                className="rounded-md border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50 disabled:opacity-50"
              >
                {icon ? "Change icon" : "Choose icon"}
              </button>
            </div>
          </div>

          <div>
            <label
              htmlFor="project-name"
              className="block text-sm font-medium text-slate-700"
            >
              Project name
            </label>
            <input
              id="project-name"
              type="text"
              value={projectName}
              onChange={(e) => setProjectName(e.target.value)}
              disabled={isLoading}
              className="mt-2 block w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm text-slate-900 outline-none transition placeholder:text-slate-400 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100 disabled:opacity-50"
              placeholder="Project name"
            />
          </div>

          <div>
            <label
              htmlFor="project-description"
              className="block text-sm font-medium text-slate-700"
            >
              Description
            </label>
            <textarea
              id="project-description"
              rows={4}
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              disabled={isLoading}
              className="mt-2 block w-full resize-none rounded-lg border border-slate-300 px-3 py-2.5 text-sm text-slate-900 outline-none transition placeholder:text-slate-400 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100 disabled:opacity-50"
              placeholder="Project description"
            />
          </div>
        </div>

        <div className="flex justify-end border-t border-slate-200 bg-slate-50 px-6 py-4">
          <button
            type="submit"
            disabled={isLoading || isSaving}
            className="rounded-lg bg-indigo-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {isSaving ? "Saving..." : "Save changes"}
          </button>
        </div>
      </form>

      {iconModalOpen && (
        <AvatarModal
          name={projectName}
          selectedAvatarUrl={icon}
          onSelect={setIcon}
          onClose={() => setIconModalOpen(false)}
        />
      )}
    </>
  );
}
