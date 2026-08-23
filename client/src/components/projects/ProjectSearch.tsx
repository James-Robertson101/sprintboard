import { useState } from "react";

function ProjectSearch() {
  const [search, setSearch] = useState("");

  return (
    <div className="w-full">
      <div className="relative w-full max-w-lg">
        {/* Search icon */}
        <svg
          className="pointer-events-none absolute left-3.5 top-1/2 h-5 w-5 -translate-y-1/2 text-slate-400"
          xmlns="http://www.w3.org/2000/svg"
          fill="none"
          viewBox="0 0 24 24"
          strokeWidth={2}
          stroke="currentColor"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            d="m21 21-4.35-4.35m2.1-5.4a7.5 7.5 0 1 1-15 0 7.5 7.5 0 0 1 15 0Z"
          />
        </svg>

        <input
          type="text"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Search projects..."
          className="
            w-full
            rounded-xl
            border border-slate-200
            bg-white
            py-3
            pl-11
            pr-10
            text-sm
            text-slate-900
            shadow-sm
            outline-none
            transition-all
            duration-200
            placeholder:text-slate-400
            hover:border-slate-300
            focus:border-indigo-500
            focus:ring-4
            focus:ring-indigo-500/10
          "
        />

        {/* Clear search */}
        {search && (
          <button
            type="button"
            onClick={() => setSearch("")}
            className="
              absolute
              right-3
              top-1/2
              flex
              h-6
              w-6
              -translate-y-1/2
              items-center
              justify-center
              rounded-full
              text-slate-400
              transition
              hover:bg-slate-100
              hover:text-slate-600
            "
            aria-label="Clear search"
          >
            <svg
              className="h-4 w-4"
              xmlns="http://www.w3.org/2000/svg"
              fill="none"
              viewBox="0 0 24 24"
              strokeWidth={2}
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M6 18 18 6M6 6l12 12"
              />
            </svg>
          </button>
        )}
      </div>
    </div>
  );
}

export default ProjectSearch;
