import { Link } from "react-router-dom";
import sprintBoardIcon from "../../assets/SprintBoard icon.png";

function TopNav() {
  return (
    <nav className="h-20 border-b border-slate-200 bg-white">
      <div className="flex h-full items-center justify-between px-6 lg:px-8">
        {/* Logo */}
        <Link
          to="/"
          className="flex items-center rounded-lg transition-opacity hover:opacity-80"
        >
          <img
            src={sprintBoardIcon}
            alt="SprintBoard"
            className="h-12 w-12 object-contain"
          />

          <span className="ml-3 hidden text-lg font-bold text-slate-900 sm:block">
            SprintBoard
          </span>
        </Link>

        {/* Profile */}
        <button className="flex items-center gap-3 rounded-lg px-3 py-2 transition hover:bg-slate-100">
          <div className="flex h-10 w-10 items-center justify-center rounded-full bg-indigo-100 text-sm font-semibold text-indigo-600">
            JD
          </div>

          <div className="hidden text-left sm:block">
            <p className="text-sm font-semibold text-slate-900">John Doe</p>

            <p className="text-xs text-slate-500">Developer</p>
          </div>

          <svg
            className="hidden h-4 w-4 text-slate-400 sm:block"
            viewBox="0 0 20 20"
            fill="currentColor"
          >
            <path
              fillRule="evenodd"
              d="M5.23 7.21a.75.75 0 011.06.02L10 11.168l3.71-3.938a.75.75 0 111.08 1.04l-4.25 4.5a.75.75 0 01-1.08 0l-4.25-4.5a.75.75 0 01.02-1.06z"
              clipRule="evenodd"
            />
          </svg>
        </button>
      </div>
    </nav>
  );
}

export default TopNav;
