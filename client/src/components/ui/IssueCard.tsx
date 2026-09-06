import type { Issue, Priority } from "../../types/Issue";

interface IssueCardProps {
  issue: Issue;
  onClick: () => void;
  onDragStart: (e: React.DragEvent<HTMLDivElement>) => void;
}

const PRIORITY_STYLES: Record<Priority, { label: string; dot: string }> = {
  Low: { label: "Low", dot: "bg-slate-400" },
  Medium: { label: "Medium", dot: "bg-sky-500" },
  High: { label: "High", dot: "bg-amber-500" },
};

function initials(name: string) {
  return name
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase())
    .join("");
}

function IssueCard({ issue, onClick, onDragStart }: IssueCardProps) {
  const priority = PRIORITY_STYLES[issue.priority];

  return (
    <div
      draggable
      onDragStart={onDragStart}
      onClick={onClick}
      className="cursor-pointer rounded-lg border border-slate-200 bg-white p-3.5 shadow-sm transition hover:border-slate-300 hover:shadow"
    >
      <h4 className="text-sm font-medium text-slate-900">{issue.name}</h4>

      {issue.description && (
        <p className="mt-1.5 line-clamp-2 text-xs text-slate-500">
          {issue.description}
        </p>
      )}

      <div className="mt-3 flex items-center justify-between">
        <span className="inline-flex items-center gap-1.5 text-xs text-slate-500">
          <span className={`h-1.5 w-1.5 rounded-full ${priority.dot}`} />
          {priority.label}
        </span>

        {issue.assignee ? (
          issue.assignee.avatarUrl ? (
            <img
              src={issue.assignee.avatarUrl}
              alt={issue.assignee.name}
              title={issue.assignee.name}
              className="h-6 w-6 rounded-full object-cover"
            />
          ) : (
            <span
              title={issue.assignee.name}
              className="flex h-6 w-6 items-center justify-center rounded-full bg-indigo-100 text-[10px] font-semibold text-indigo-700"
            >
              {initials(issue.assignee.name)}
            </span>
          )
        ) : (
          <span
            title="Unassigned"
            className="flex h-6 w-6 items-center justify-center rounded-full border border-dashed border-slate-300 text-[10px] text-slate-400"
          >
            —
          </span>
        )}
      </div>
    </div>
  );
}

export default IssueCard;
