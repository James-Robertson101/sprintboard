import { useState } from "react";
import type { Issue, IssueStatus } from "../../types/Issue";
import IssueCard from "./IssueCard";

interface BoardColumnProps {
  status: IssueStatus;
  label: string;
  issues: Issue[];
  onIssueClick: (issue: Issue) => void;
  onDropIssue: (issueId: number, status: IssueStatus) => void;
  onAddIssue: (status: IssueStatus) => void;
}

function BoardColumn({
  status,
  label,
  issues,
  onIssueClick,
  onDropIssue,
  onAddIssue,
}: BoardColumnProps) {
  const [isDragOver, setIsDragOver] = useState(false);

  return (
    <div className="flex w-72 shrink-0 flex-col sm:w-80">
      <div className="mb-3 flex items-center justify-between px-1">
        <h3 className="text-sm font-semibold text-slate-900">{label}</h3>
        <span className="rounded-full bg-slate-100 px-2 py-0.5 text-xs font-medium text-slate-500">
          {issues.length}
        </span>
      </div>

      <div
        onDragOver={(e) => {
          e.preventDefault();
          setIsDragOver(true);
        }}
        onDragLeave={() => setIsDragOver(false)}
        onDrop={(e) => {
          e.preventDefault();
          setIsDragOver(false);
          const issueId = Number(e.dataTransfer.getData("text/issue-id"));
          if (!Number.isNaN(issueId)) {
            onDropIssue(issueId, status);
          }
        }}
        className={`flex min-h-32 flex-1 flex-col gap-2.5 rounded-xl border p-2.5 transition ${
          isDragOver
            ? "border-indigo-300 bg-indigo-50/60"
            : "border-slate-200 bg-slate-100/60"
        }`}
      >
        {issues.map((issue) => (
          <IssueCard
            key={issue.id}
            issue={issue}
            onClick={() => onIssueClick(issue)}
            onDragStart={(e) => {
              e.dataTransfer.setData("text/issue-id", String(issue.id));
              e.dataTransfer.effectAllowed = "move";
            }}
          />
        ))}

        <button
          type="button"
          onClick={() => onAddIssue(status)}
          className="mt-1 rounded-lg border border-dashed border-slate-300 px-3 py-2 text-left text-xs font-medium text-slate-400 transition hover:border-slate-400 hover:text-slate-600"
        >
          + Add issue
        </button>
      </div>
    </div>
  );
}

export default BoardColumn;
