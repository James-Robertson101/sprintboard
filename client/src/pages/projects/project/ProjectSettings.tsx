import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import {
  getProjectMembers,
  removeProjectMember,
} from "../../../services/projectService";
import type { ProjectMember } from "../../../types/project";

type SettingsSection = "general" | "members" | "danger";

function ProjectSettings() {
  const { projectId } = useParams();
  const navigate = useNavigate();

  const [activeSection, setActiveSection] =
    useState<SettingsSection>("general");

  const [members, setMembers] = useState<ProjectMember[]>([]);
  const [isLoadingMembers, setIsLoadingMembers] = useState(false);
  const [membersError, setMembersError] = useState<string | null>(null);

  const [projectName, setProjectName] = useState("Project");
  const [description, setDescription] = useState("");

  const [username, setUsername] = useState("");
  const [isAddingMember, setIsAddingMember] = useState(false);

  useEffect(() => {
    if (!projectId) return;

    const id = projectId;

    async function loadMembers() {
      setIsLoadingMembers(true);
      setMembersError(null);

      try {
        const data = await getProjectMembers(id);
        setMembers(data);
      } catch (error) {
        setMembersError(
          error instanceof Error
            ? error.message
            : "Failed to load project members.",
        );
      } finally {
        setIsLoadingMembers(false);
      }
    }

    loadMembers();
  }, [projectId]);

  async function handleAddMember(e: React.SubmitEvent) {
    e.preventDefault();

    if (!projectId || !username.trim()) return;

    setIsAddingMember(true);
    setMembersError(null);

    try {
      // TODO:
      // const member = await addProjectMember(projectId, username.trim());
      // setMembers((current) => [...current, member]);

      setUsername("");
    } catch (error) {
      setMembersError(
        error instanceof Error
          ? error.message
          : "Failed to add project member.",
      );
    } finally {
      setIsAddingMember(false);
    }
  }

  async function handleRemoveMember(memberId: number) {
    if (!projectId) return;

    const confirmed = window.confirm(
      "Are you sure you want to remove this member from the project?",
    );

    if (!confirmed) return;

    try {
      // TODO:
      await removeProjectMember(Number(projectId), memberId);

      setMembers((current) =>
        current.filter((member) => member.id !== memberId),
      );
    } catch (error) {
      setMembersError(
        error instanceof Error
          ? error.message
          : "Failed to remove project member.",
      );
    }
  }

  function renderIcon(section: SettingsSection) {
    if (section === "general") {
      return (
        <svg
          className="h-5 w-5"
          fill="none"
          viewBox="0 0 24 24"
          strokeWidth={1.8}
          stroke="currentColor"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            d="M9.594 3.94c.09-.542.56-.94 1.11-.94h2.592c.55 0 1.02.398 1.11.94l.213 1.276c.063.38.313.705.65.88.146.076.29.157.43.242.33.201.74.27 1.1.15l1.24-.413a1.125 1.125 0 011.343.52l1.296 2.247c.275.477.196 1.08-.19 1.47l-.92.92c-.276.276-.37.68-.24 1.04.05.137.095.276.135.416.104.365.42.647.79.714l1.27.23c.54.098.93.568.93 1.117v2.593c0 .55-.39 1.02-.93 1.117l-1.27.23c-.37.067-.686.35-.79.714-.04.14-.085.28-.136.417-.13.36-.035.763.24 1.04l.92.919c.386.387.465.99.19 1.468l-1.296 2.247a1.125 1.125 0 01-1.343.52l-1.24-.412a1.125 1.125 0 00-1.1.15c-.14.085-.284.166-.43.242a1.125 1.125 0 00-.65.88l-.213 1.276c-.09.542-.56.94-1.11.94h-2.592c-.55 0-1.02-.398-1.11-.94l-.213-1.276a1.125 1.125 0 00-.65-.88 8.28 8.28 0 01-.43-.242 1.125 1.125 0 00-1.1-.15l-1.24.413a1.125 1.125 0 01-1.343-.52L3.51 20.57a1.125 1.125 0 01.19-1.468l.92-.92c.276-.276.37-.68.24-1.04a8.28 8.28 0 01-.135-.416 1.125 1.125 0 00-.79-.714l-1.27-.23a1.125 1.125 0 01-.93-1.117v-2.593c0-.55.39-1.02.93-1.117l1.27-.23c.37-.067.686-.35.79-.714.04-.14.085-.28.136-.417.13-.36.035-.763-.24-1.04l-.92-.919a1.125 1.125 0 01-.19-1.468l1.296-2.247a1.125 1.125 0 011.343-.52l1.24.412a1.125 1.125 0 001.1-.15c.14-.085.284-.166.43-.242.337-.175.587-.5.65-.88l.213-1.276z"
          />
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
          />
        </svg>
      );
    }

    if (section === "members") {
      return (
        <svg
          className="h-5 w-5"
          fill="none"
          viewBox="0 0 24 24"
          strokeWidth={1.8}
          stroke="currentColor"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            d="M15 19.128a9.38 9.38 0 005.75-2.122M15 19.128v-3.056a4.5 4.5 0 00-4.5-4.5H7.5a4.5 4.5 0 00-4.5 4.5v3.056m12 0a9.38 9.38 0 01-12 0M12 7.5a3.75 3.75 0 11-7.5 0A3.75 3.75 0 0112 7.5zm9 3a2.25 2.25 0 11-4.5 0 2.25 2.25 0 014.5 0z"
          />
        </svg>
      );
    }

    return (
      <svg
        className="h-5 w-5"
        fill="none"
        viewBox="0 0 24 24"
        strokeWidth={1.8}
        stroke="currentColor"
      >
        <path
          strokeLinecap="round"
          strokeLinejoin="round"
          d="M12 9v4m0 4h.01M10.29 3.86l-7.82 13.5A1.5 1.5 0 003.77 19.5h16.46a1.5 1.5 0 001.3-2.25l-7.82-13.5a1.5 1.5 0 00-2.6 0z"
        />
      </svg>
    );
  }

  return (
    <main className="min-w-0 flex-1">
      <div className="mx-auto max-w-6xl px-6 py-8 lg:px-10">
        {/* Header */}
        <div className="mb-8">
          <button
            type="button"
            onClick={() => navigate(`/projects/${projectId}/board`)}
            className="mb-4 inline-flex items-center gap-2 text-sm font-medium text-slate-500 transition hover:text-slate-900"
          >
            <svg
              className="h-4 w-4"
              fill="none"
              viewBox="0 0 24 24"
              strokeWidth={2}
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M15 19l-7-7 7-7"
              />
            </svg>
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
          {/* Settings navigation */}
          <aside>
            <nav className="rounded-xl border border-slate-200 bg-white p-2 shadow-sm">
              <button
                type="button"
                onClick={() => setActiveSection("general")}
                className={`flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-left text-sm font-medium transition ${
                  activeSection === "general"
                    ? "bg-indigo-50 text-indigo-700"
                    : "text-slate-600 hover:bg-slate-50 hover:text-slate-900"
                }`}
              >
                {renderIcon("general")}
                General
              </button>

              <button
                type="button"
                onClick={() => setActiveSection("members")}
                className={`flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-left text-sm font-medium transition ${
                  activeSection === "members"
                    ? "bg-indigo-50 text-indigo-700"
                    : "text-slate-600 hover:bg-slate-50 hover:text-slate-900"
                }`}
              >
                {renderIcon("members")}
                Members
              </button>

              <div className="my-2 border-t border-slate-100" />

              <button
                type="button"
                onClick={() => setActiveSection("danger")}
                className={`flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-left text-sm font-medium transition ${
                  activeSection === "danger"
                    ? "bg-rose-50 text-rose-700"
                    : "text-slate-600 hover:bg-slate-50 hover:text-slate-900"
                }`}
              >
                {renderIcon("danger")}
                Danger zone
              </button>
            </nav>
          </aside>

          {/* Content */}
          <section className="min-w-0">
            {activeSection === "general" && (
              <div className="rounded-xl border border-slate-200 bg-white shadow-sm">
                <div className="border-b border-slate-200 px-6 py-5">
                  <h2 className="text-base font-semibold text-slate-900">
                    General
                  </h2>

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
            )}

            {activeSection === "members" && (
              <div className="space-y-6">
                {/* Add member */}
                <div className="rounded-xl border border-slate-200 bg-white shadow-sm">
                  <div className="border-b border-slate-200 px-6 py-5">
                    <h2 className="text-base font-semibold text-slate-900">
                      Add a member
                    </h2>

                    <p className="mt-1 text-sm text-slate-500">
                      Add someone to this project using their username.
                    </p>
                  </div>

                  <form onSubmit={handleAddMember} className="px-6 py-6">
                    <div className="flex flex-col gap-3 sm:flex-row">
                      <div className="min-w-0 flex-1">
                        <label htmlFor="member-username" className="sr-only">
                          Username
                        </label>

                        <input
                          id="member-username"
                          type="text"
                          value={username}
                          onChange={(e) => setUsername(e.target.value)}
                          placeholder="Enter username"
                          className="block w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm text-slate-900 outline-none transition placeholder:text-slate-400 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
                        />
                      </div>

                      <button
                        type="submit"
                        disabled={isAddingMember || !username.trim()}
                        className="inline-flex items-center justify-center rounded-lg bg-indigo-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-indigo-700 disabled:cursor-not-allowed disabled:opacity-50"
                      >
                        {isAddingMember ? "Adding..." : "Add member"}
                      </button>
                    </div>
                  </form>
                </div>

                {/* Member list */}
                <div className="rounded-xl border border-slate-200 bg-white shadow-sm">
                  <div className="border-b border-slate-200 px-6 py-5">
                    <div className="flex items-center justify-between gap-4">
                      <div>
                        <h2 className="text-base font-semibold text-slate-900">
                          Project members
                        </h2>

                        <p className="mt-1 text-sm text-slate-500">
                          People who have access to this project.
                        </p>
                      </div>

                      <span className="rounded-full bg-slate-100 px-2.5 py-1 text-xs font-medium text-slate-600">
                        {members.length}{" "}
                        {members.length === 1 ? "member" : "members"}
                      </span>
                    </div>
                  </div>

                  {membersError && (
                    <div className="mx-6 mt-5 rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
                      {membersError}
                    </div>
                  )}

                  {isLoadingMembers ? (
                    <div className="px-6 py-10 text-center text-sm text-slate-500">
                      Loading members...
                    </div>
                  ) : members.length === 0 ? (
                    <div className="px-6 py-12 text-center">
                      <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-indigo-50">
                        <svg
                          className="h-6 w-6 text-indigo-600"
                          fill="none"
                          viewBox="0 0 24 24"
                          strokeWidth={1.5}
                          stroke="currentColor"
                        >
                          <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            d="M15 19.128a9.38 9.38 0 005.75-2.122M15 19.128v-3.056a4.5 4.5 0 00-4.5-4.5H7.5a4.5 4.5 0 00-4.5 4.5v3.056m12 0a9.38 9.38 0 01-12 0M12 7.5a3.75 3.75 0 11-7.5 0A3.75 3.75 0 0112 7.5zm9 3a2.25 2.25 0 11-4.5 0 2.25 2.25 0 014.5 0z"
                          />
                        </svg>
                      </div>

                      <h3 className="mt-4 text-sm font-semibold text-slate-900">
                        No members yet
                      </h3>

                      <p className="mt-1 text-sm text-slate-500">
                        Add your first member using the form above.
                      </p>
                    </div>
                  ) : (
                    <div className="divide-y divide-slate-200">
                      {members.map((member) => (
                        <div
                          key={member.id}
                          className="flex items-center justify-between gap-4 px-6 py-4"
                        >
                          <div className="flex min-w-0 items-center gap-3">
                            {member.avatarUrl ? (
                              <img
                                src={member.avatarUrl}
                                alt={member.name}
                                className="h-10 w-10 shrink-0 rounded-full object-cover"
                              />
                            ) : (
                              <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-indigo-100 text-sm font-semibold text-indigo-700">
                                {member.name.charAt(0).toUpperCase()}
                              </div>
                            )}

                            <div className="min-w-0">
                              <p className="truncate text-sm font-medium text-slate-900">
                                {member.name}
                              </p>

                              <p className="text-sm text-slate-500">
                                {member.role}
                              </p>
                            </div>
                          </div>

                          <div className="flex shrink-0 items-center gap-3">
                            <span
                              className={`rounded-full px-2.5 py-1 text-xs font-medium ${
                                member.role === "Owner"
                                  ? "bg-indigo-50 text-indigo-700"
                                  : "bg-slate-100 text-slate-600"
                              }`}
                            >
                              {member.role}
                            </span>

                            {member.role !== "Owner" && (
                              <button
                                type="button"
                                onClick={() => handleRemoveMember(member.id)}
                                className="rounded-lg p-2 text-slate-400 transition hover:bg-rose-50 hover:text-rose-600"
                                title="Remove member"
                              >
                                <svg
                                  className="h-5 w-5"
                                  fill="none"
                                  viewBox="0 0 24 24"
                                  strokeWidth={1.8}
                                  stroke="currentColor"
                                >
                                  <path
                                    strokeLinecap="round"
                                    strokeLinejoin="round"
                                    d="M6 18L18 6M6 6l12 12"
                                  />
                                </svg>
                              </button>
                            )}
                          </div>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </div>
            )}

            {activeSection === "danger" && (
              <div className="rounded-xl border border-rose-200 bg-white shadow-sm">
                <div className="border-b border-rose-100 px-6 py-5">
                  <h2 className="text-base font-semibold text-rose-700">
                    Danger zone
                  </h2>

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
                      Permanently delete this project and its issues. This
                      action cannot be undone.
                    </p>
                  </div>

                  <button
                    type="button"
                    className="shrink-0 rounded-lg border border-rose-300 px-4 py-2.5 text-sm font-semibold text-rose-600 transition hover:bg-rose-50"
                  >
                    Delete project
                  </button>
                </div>
              </div>
            )}
          </section>
        </div>
      </div>
    </main>
  );
}

export default ProjectSettings;
