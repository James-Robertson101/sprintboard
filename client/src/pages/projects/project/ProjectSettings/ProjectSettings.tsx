import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { SettingsNav, type SettingsSection } from "./SettingsNav";
import { GeneralSettingsPanel } from "./GeneralSettingsPanel";
import { MembersSettingsPanel } from "./MembersSettingsPanel";
import { DangerZonePanel } from "./DangerZonePanel";
import { BackArrowIcon } from "./icons";

function ProjectSettings() {
  const { projectId } = useParams();
  const [activeSection, setActiveSection] =
    useState<SettingsSection>("general");
  const navigate = useNavigate();

  if (!projectId) return null;

  return (
    <main className="min-w-0 flex-1">
      <div className="mx-auto max-w-6xl px-6 py-8 lg:px-10">
        <div className="mb-8">
          <button
            type="button"
            onClick={() => navigate(`/projects/${projectId}/board`)}
            className="mb-4 inline-flex items-center gap-2 text-sm font-medium text-slate-500 transition hover:text-slate-900"
          >
            <BackArrowIcon />
            Back to board
          </button>

          <p className="mb-1 text-sm font-medium text-indigo-600">Project</p>
          <h1 className="text-3xl font-bold tracking-tight text-slate-900">
            Project settings
          </h1>
          <p className="mt-2 text-sm text-slate-500">
            Manage your project details and members.
          </p>
        </div>

        <div className="grid grid-cols-1 gap-6 lg:grid-cols-[220px_minmax(0,1fr)]">
          <aside>
            <SettingsNav
              activeSection={activeSection}
              onSectionChange={setActiveSection}
            />
          </aside>

          <section className="min-w-0">
            {activeSection === "general" && <GeneralSettingsPanel />}
            {activeSection === "members" && (
              <MembersSettingsPanel projectId={projectId} />
            )}
            {activeSection === "danger" && (
              <DangerZonePanel projectId={projectId} />
            )}
          </section>
        </div>
      </div>
    </main>
  );
}

export default ProjectSettings;
