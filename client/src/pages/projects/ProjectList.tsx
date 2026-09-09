import { useEffect, useState } from "react";
import TopNav from "../../components/layout/TopNav";
import ProjectSearch from "../../components/projects/ProjectSearch";
import type { Project } from "../../types/project";
import { getProjects } from "../../services/projectService";
import { Link } from "react-router-dom";
import { useAuth } from "../../context/useAuth";
import Sidebar from "../../components/layout/Sidebar";

function ProjectList() {
  const [search, setSearch] = useState("");
  const [submittedSearch, setSubmittedSearch] = useState("");
  const [projectList, setProjectList] = useState<Project[]>([]);

  const { user } = useAuth();

  function handleSubmit() {
    setSubmittedSearch(search);
  }

  useEffect(() => {
    async function loadProjects() {
      try {
        const projects = await getProjects(submittedSearch);
        setProjectList(projects);
      } catch (error) {
        console.error("Failed to load projects:", error);
      }
    }

    loadProjects();
  }, [submittedSearch]);

  const buttonStyles =
    "inline-flex items-center justify-center gap-2 rounded-lg bg-indigo-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2";

  return (
    <div className="min-h-screen bg-slate-50">
      <TopNav
        name={user?.name}
        email={user?.email}
        avatarUrl={user?.avatarUrl}
      />

      <div className="flex">
        {/* Sidebar */}
        <Sidebar />

        {/* Main content */}
        <main className="min-w-0 flex-1">
          <div className="mx-auto max-w-7xl px-6 py-8 lg:px-10">
            {/* Page heading */}
            <div className="mb-8 flex flex-col gap-5 sm:flex-row sm:items-end sm:justify-between">
              <div>
                <p className="mb-1 text-sm font-medium text-indigo-600">
                  Workspace
                </p>

                <h1 className="text-3xl font-bold tracking-tight text-slate-900 sm:text-4xl">
                  Projects
                </h1>

                <p className="mt-2 text-sm text-slate-500">
                  Manage and keep track of all your projects.
                </p>
              </div>

              <Link to={`/projects/create`}>
                <button type="button" className={buttonStyles}>
                  <svg
                    className="h-5 w-5"
                    xmlns="http://www.w3.org/2000/svg"
                    fill="none"
                    viewBox="0 0 24 24"
                    strokeWidth={2}
                    stroke="currentColor"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      d="M12 4.5v15m7.5-7.5h-15"
                    />
                  </svg>
                  New Project
                </button>
              </Link>
            </div>

            {/* Search */}
            <div className="mb-6 rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
              <ProjectSearch
                search={search}
                setSearch={setSearch}
                onSubmit={handleSubmit}
              />
            </div>

            {/* Projects */}
            <div className="rounded-xl border border-slate-200 bg-white shadow-sm">
              <div className="border-b border-slate-200 px-6 py-4">
                <h2 className="text-base font-semibold text-slate-900">
                  Your Projects
                </h2>

                <p className="mt-1 text-sm text-slate-500">
                  Select a project to view its details.
                </p>
              </div>

              {projectList.length === 0 ? (
                submittedSearch ? (
                  /* No search results */
                  <div className="px-6 py-12 text-center">
                    <h3 className="text-sm font-semibold text-slate-900">
                      No projects found
                    </h3>

                    <p className="mt-1 text-sm text-slate-500">
                      No projects matched "{submittedSearch}".
                    </p>
                  </div>
                ) : (
                  /* No projects */
                  <div className="px-6 py-12 text-center">
                    <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-indigo-50">
                      <svg
                        className="h-6 w-6 text-indigo-600"
                        xmlns="http://www.w3.org/2000/svg"
                        fill="none"
                        viewBox="0 0 24 24"
                        strokeWidth={1.5}
                        stroke="currentColor"
                      >
                        <path
                          strokeLinecap="round"
                          strokeLinejoin="round"
                          d="M3.75 6.75h16.5M3.75 12h16.5m-16.5 5.25h16.5"
                        />
                      </svg>
                    </div>

                    <h3 className="mt-4 text-sm font-semibold text-slate-900">
                      No projects yet
                    </h3>

                    <p className="mx-auto mt-1 max-w-sm text-sm text-slate-500">
                      Create your first project to start organising your work.
                    </p>

                    <Link to={`/projects/create`}>
                      <button type="button" className={`mt-5 ${buttonStyles}`}>
                        <svg
                          className="h-5 w-5"
                          xmlns="http://www.w3.org/2000/svg"
                          fill="none"
                          viewBox="0 0 24 24"
                          strokeWidth={2}
                          stroke="currentColor"
                        >
                          <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            d="M12 4.5v15m7.5-7.5h-15"
                          />
                        </svg>
                        Create your first project
                      </button>
                    </Link>
                  </div>
                )
              ) : (
                /* Project list */
                <div className="divide-y divide-slate-200">
                  {projectList.map((project) => (
                    <Link
                      key={project.id}
                      to={`/projects/${project.id}`}
                      className="block px-6 py-4 transition hover:bg-slate-50"
                    >
                      <h3 className="font-medium text-slate-900">
                        {project.name}
                      </h3>

                      {project.description && (
                        <p className="mt-1 text-sm text-slate-500">
                          {project.description}
                        </p>
                      )}
                    </Link>
                  ))}
                </div>
              )}
            </div>
          </div>
        </main>
      </div>
    </div>
  );
}

export default ProjectList;
