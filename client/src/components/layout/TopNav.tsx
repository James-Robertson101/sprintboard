import { Link } from "react-router-dom";
import sprintBoardIcon from "../../assets/SprintBoard icon.png";
import { useAuth } from "../../context/useAuth";
import { useNavigate } from "react-router-dom";

type TopNavProps = {
  name?: string;
  email?: string;
  avatarUrl?: string;
};

function TopNav({ name, email, avatarUrl }: TopNavProps) {
  const { logout } = useAuth();
  const navigate = useNavigate();
  async function handleLogout() {
    try {
      await logout();
      navigate("/login", { replace: true });
    } catch (error) {
      console.error("Logout failed:", error);
      // still navigate away / show a toast, your call
    }
  }
  return (
    <nav className="h-20 border-b border-slate-200 bg-white">
      <div className="flex h-full items-center justify-between px-6 lg:px-8">
        {/* Logo */}
        <Link
          to="/projects"
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
        <button
          type="button"
          className="flex items-center gap-3 rounded-lg px-3 py-2 transition hover:bg-slate-100"
        >
          {/* Avatar */}
          <div
            onClick={handleLogout}
            className="flex h-10 w-10 shrink-0 items-center justify-center overflow-hidden rounded-full bg-indigo-100 text-sm font-semibold text-indigo-600"
          >
            {avatarUrl ? (
              <img
                src={avatarUrl}
                alt={`${name ?? "User"} avatar`}
                className="h-full w-full object-cover"
              />
            ) : (
              <span>{name?.charAt(0).toUpperCase() ?? "?"}</span>
            )}
          </div>

          {/* User details */}
          <div className="hidden text-left sm:block">
            <p className="text-sm font-semibold text-slate-900">
              {name ?? "Loading ..."}
            </p>

            <p className="text-xs text-slate-500">{email ?? ""}</p>
          </div>

          {/* Dropdown arrow */}
          <svg
            className="hidden h-4 w-4 text-slate-400 sm:block"
            viewBox="0 0 20 20"
            fill="currentColor"
          >
            <path
              fillRule="evenodd"
              d="M5.23 7.21a.75.75 0 011.06.02L10 11.168l3.71-3.938a.75.75 0 111.08 1.04l-4.25 4.5a.75.75 0 01-1.08 0l-4.25-4.5a.75.75 0 01-1.08 0z"
              clipRule="evenodd"
            />
          </svg>
        </button>
      </div>
    </nav>
  );
}

export default TopNav;
