import type { UserSummary } from "../../../../types/project";

interface UserSearchResultRowProps {
  user: UserSummary;
  isAdding: boolean;
  onAdd: (user: UserSummary) => void;
}

export function UserSearchResultRow({
  user,
  isAdding,
  onAdd,
}: UserSearchResultRowProps) {
  return (
    <div className="flex items-center justify-between gap-4 px-4 py-3">
      <div className="flex min-w-0 items-center gap-3">
        {user.avatarUrl ? (
          <img
            src={user.avatarUrl}
            alt={user.name}
            className="h-8 w-8 shrink-0 rounded-full object-cover"
          />
        ) : (
          <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-indigo-100 text-xs font-semibold text-indigo-700">
            {user.name.charAt(0).toUpperCase()}
          </div>
        )}
        <p className="truncate text-sm font-medium text-slate-900">
          {user.name}
        </p>
      </div>

      <button
        type="button"
        onClick={() => onAdd(user)}
        disabled={isAdding}
        className="shrink-0 rounded-lg bg-indigo-50 px-3 py-1.5 text-xs font-semibold text-indigo-700 transition hover:bg-indigo-100 disabled:cursor-not-allowed disabled:opacity-50"
      >
        {isAdding ? "Adding..." : "Add"}
      </button>
    </div>
  );
}
