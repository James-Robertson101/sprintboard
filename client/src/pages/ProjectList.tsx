import TopNav from "../components/layout/TopNav";
import ProjectSearch from "../components/projects/ProjectSearch";

function ProjectList() {
  return (
    <div className="min-h-screen bg-slate-50">
      <TopNav />

      <div className="flex">
        {/* Sidebar */}
        <aside className="hidden min-h-[calc(100vh-5rem)] w-64 shrink-0 border-r border-slate-200 bg-white md:block">
          <div className="p-4">
            <nav className="space-y-1">
              <a
                href="#"
                className="flex items-center rounded-lg bg-indigo-50 px-4 py-3 text-sm font-medium text-indigo-700"
              >
                Projects
              </a>

              <a
                href="#"
                className="flex items-center rounded-lg px-4 py-3 text-sm font-medium text-slate-600 transition hover:bg-slate-50 hover:text-slate-900"
              >
                Dashboard
              </a>

              <a
                href="#"
                className="flex items-center rounded-lg px-4 py-3 text-sm font-medium text-slate-600 transition hover:bg-slate-50 hover:text-slate-900"
              >
                Settings
              </a>
            </nav>
          </div>
        </aside>

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

              {/* New Project button */}
              <button
                type="button"
                className="inline-flex items-center justify-center gap-2 rounded-lg bg-indigo-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
              >
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
            </div>

            {/* Search */}
            <div className="mb-6 rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
              <ProjectSearch />
            </div>

            {/* Projects content */}
            <div className="rounded-xl border border-slate-200 bg-white shadow-sm">
              <div className="border-b border-slate-200 px-6 py-4">
                <h2 className="text-base font-semibold text-slate-900">
                  Your Projects
                </h2>

                <p className="mt-1 text-sm text-slate-500">
                  Select a project to view its details.
                </p>
              </div>

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

                <button
                  type="button"
                  className="mt-5 inline-flex items-center gap-2 rounded-lg bg-indigo-600 px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
                >
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
              </div>
            </div>
          </div>
        </main>
      </div>
    </div>
  );
}

export default ProjectList;
