import { NavLink } from "react-router-dom";

function Sidebar() {
  return (
    <aside className="hidden min-h-[calc(100vh-5rem)] w-64 shrink-0 border-r border-slate-200 bg-white md:block">
      <nav className="space-y-1 p-4">
        <NavLink
          to="/projects"
          className={({ isActive }) =>
            `flex items-center rounded-lg px-4 py-3 text-sm font-medium ${
              isActive
                ? "bg-indigo-50 text-indigo-700"
                : "text-slate-600 transition hover:bg-slate-50 hover:text-slate-900"
            }`
          }
        >
          Projects
        </NavLink>

        <NavLink
          to="/dashboard"
          className={({ isActive }) =>
            `flex items-center rounded-lg px-4 py-3 text-sm font-medium ${
              isActive
                ? "bg-indigo-50 text-indigo-700"
                : "text-slate-600 transition hover:bg-slate-50 hover:text-slate-900"
            }`
          }
        >
          Dashboard
        </NavLink>
      </nav>
    </aside>
  );
}

export default Sidebar;
