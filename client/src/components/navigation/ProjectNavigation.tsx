import { NavLink } from "react-router-dom";

function ProjectNavigation() {
  const navigation = [
    { name: "Board", to: "board" },
    { name: "Backlog", to: "backlog" },
    { name: "Issues", to: "issues" },
    { name: "Sprints", to: "sprints" },
    { name: "Reports", to: "reports" },
    { name: "Project Settings", to: "projectSettings" },
  ];

  return (
    <nav className="flex w-full border-b border-slate-200 bg-white px-6">
      <div className="flex gap-6">
        {navigation.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              `border-b-2 px-1 py-4 text-sm font-medium transition ${
                isActive
                  ? "border-indigo-600 text-indigo-600"
                  : "border-transparent text-slate-600 hover:text-slate-900"
              }`
            }
          >
            {item.name}
          </NavLink>
        ))}
      </div>
    </nav>
  );
}

export default ProjectNavigation;
