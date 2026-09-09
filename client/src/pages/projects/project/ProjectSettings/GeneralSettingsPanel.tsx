import { useState } from "react";

export function GeneralSettingsPanel() {
  const [projectName, setProjectName] = useState("Project");
  const [description, setDescription] = useState("");

  return (
    <div className="rounded-xl border border-slate-200 bg-white shadow-sm">
      <div className="border-b border-slate-200 px-6 py-5">
        <h2 className="text-base font-semibold text-slate-900">General</h2>
        <p className="mt-1 text-sm text-slate-500">
          Update the basic information for this project.
        </p>
      </div>

      <div className="space-y-6 px-6 py-6">
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
            className="mt-2 block w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm text-slate-900 outline-none transition placeholder:text-slate-400 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
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
            className="mt-2 block w-full resize-none rounded-lg border border-slate-300 px-3 py-2.5 text-sm text-slate-900 outline-none transition placeholder:text-slate-400 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
            placeholder="Project description"
          />
        </div>
      </div>

      <div className="flex justify-end border-t border-slate-200 bg-slate-50 px-6 py-4">
        <button
          type="button"
          className="rounded-lg bg-indigo-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
        >
          Save changes
        </button>
      </div>
    </div>
  );
}
