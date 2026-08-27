import { useState } from "react";
import AvatarModal from "../../components/auth/AvatarModal";
import Button from "../../components/ui/Button";
import TopNav from "../../components/layout/TopNav";
import { createProject } from "../../services/projectService";
import type { ProjectData } from "../../types/project";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../context/useAuth";

function CreateProject() {
  const [projectName, setProjectName] = useState("");
  const [description, setDescription] = useState("");
  const [avatarUrl, setAvatarUrl] = useState("");
  const [avatarModalOpen, setAvatarModalOpen] = useState(false);

  const navigate = useNavigate();
  const { user } = useAuth();

  async function handleSubmit(e: React.SubmitEvent<HTMLFormElement>) {
    e.preventDefault();

    const projectData: ProjectData = {
      name: projectName,
      description: description,
      avatarUrl: avatarUrl,
    };

    try {
      await createProject(projectData);
      navigate("/projects");
    } catch (error) {
      console.log("failed to create project: ", error);
    }
  }

  return (
    <div className="min-h-screen bg-slate-50">
      <TopNav
        name={user?.name}
        email={user?.email}
        avatarUrl={user?.avatarUrl}
      />

      <main className="mx-auto flex w-full max-w-3xl justify-center px-6 py-10 lg:px-8">
        <div className="w-full">
          {/* Page heading */}
          <div className="mb-8">
            <p className="mb-1 text-sm font-medium text-indigo-600">
              Workspace
            </p>

            <h1 className="text-3xl font-bold tracking-tight text-slate-900 sm:text-4xl">
              Create Project
            </h1>

            <p className="mt-2 text-sm text-slate-500">
              Create a new project and start organising your work.
            </p>
          </div>

          {/* Form card */}
          <form
            onSubmit={handleSubmit}
            className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm sm:p-8"
          >
            {/* Project name */}
            <div>
              <label
                htmlFor="projectName"
                className="block text-sm font-semibold text-slate-900"
              >
                Project name
              </label>

              <p className="mt-1 text-sm text-slate-500">
                Choose a name that clearly identifies your project.
              </p>

              <input
                id="projectName"
                type="text"
                value={projectName}
                onChange={(e) => setProjectName(e.target.value)}
                placeholder="Enter your project name"
                required
                className="mt-3 block w-full rounded-lg border border-slate-300 bg-white px-4 py-2.5 text-sm text-slate-900 shadow-sm outline-none transition placeholder:text-slate-400 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/20"
              />
            </div>

            {/* Description */}
            <div className="mt-6">
              <label
                htmlFor="description"
                className="block text-sm font-semibold text-slate-900"
              >
                Description
              </label>

              <p className="mt-1 text-sm text-slate-500">
                Add a short description of what this project is about.
              </p>

              <textarea
                id="description"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="Enter the project description"
                rows={4}
                className="mt-3 block w-full resize-none rounded-lg border border-slate-300 bg-white px-4 py-2.5 text-sm text-slate-900 shadow-sm outline-none transition placeholder:text-slate-400 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/20"
              />
            </div>

            {/* Avatar */}
            <div className="mt-6">
              <label className="block text-sm font-semibold text-slate-900">
                Project avatar
              </label>

              <p className="mt-1 text-sm text-slate-500">
                Choose an avatar to help identify your project.
              </p>

              <div className="mt-4 flex items-center gap-4">
                {avatarUrl ? (
                  <img
                    src={avatarUrl}
                    alt="Selected project avatar"
                    className="h-16 w-16 rounded-full border border-slate-200 object-cover shadow-sm"
                  />
                ) : (
                  <div className="flex h-16 w-16 items-center justify-center rounded-full border border-dashed border-slate-300 bg-slate-50 text-xs font-medium text-slate-400">
                    None
                  </div>
                )}

                <button
                  type="button"
                  onClick={() => setAvatarModalOpen(true)}
                  className="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 shadow-sm transition hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
                >
                  {avatarUrl ? "Change avatar" : "Choose avatar"}
                </button>
              </div>
            </div>

            {/* Actions */}
            <div className="mt-8 flex items-center justify-end gap-3 border-t border-slate-200 pt-6">
              <button
                type="button"
                onClick={() => navigate("/projects")}
                className="rounded-lg px-4 py-2.5 text-sm font-semibold text-slate-600 transition hover:bg-slate-100 hover:text-slate-900"
              >
                Cancel
              </button>

              <Button type="submit">Create Project</Button>
            </div>
          </form>
        </div>
      </main>

      {avatarModalOpen && (
        <AvatarModal
          name={projectName}
          selectedAvatarUrl={avatarUrl}
          onSelect={setAvatarUrl}
          onClose={() => setAvatarModalOpen(false)}
        />
      )}
    </div>
  );
}

export default CreateProject;
