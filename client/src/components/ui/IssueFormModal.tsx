import { useEffect, useState } from "react";
import type {
  CreateIssuePayload,
  Issue,
  IssueStatus,
  Priority,
  UpdateIssuePayload,
} from "../../types/Issue";

interface IssueFormModalProps {
  mode: "create" | "edit";
  initialStatus: IssueStatus;
  issue?: Issue;
  onClose: () => void;
  onCreate: (payload: CreateIssuePayload) => Promise<void>;
  onUpdate: (payload: UpdateIssuePayload) => Promise<void>;
}

const PRIORITY_OPTIONS: Priority[] = ["Low", "Medium", "High"];
const STATUS_OPTIONS: { value: IssueStatus; label: string }[] = [
  { value: "Todo", label: "To do" },
  { value: "InProgress", label: "In progress" },
  { value: "InReview", label: "In review" },
  { value: "Done", label: "Done" },
];

const inputStyles =
  "w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-900 shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500";
const labelStyles = "mb-1.5 block text-sm font-medium text-slate-700";

function IssueFormModal({
  mode,
  initialStatus,
  issue,
  onClose,
  onCreate,
  onUpdate,
}: IssueFormModalProps) {
  const [name, setName] = useState(issue?.name ?? "");
  const [description, setDescription] = useState(issue?.description ?? "");
  const [priority, setPriority] = useState<Priority>(
    issue?.priority ?? "Medium",
  );
  const [status, setStatus] = useState<IssueStatus>(
    issue?.status ?? initialStatus,
  );
  const [assigneeId, setAssigneeId] = useState(
    issue?.assignee ? String(issue.assignee.id) : "",
  );
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    function handleKeyDown(e: KeyboardEvent) {
      if (e.key === "Escape") onClose();
    }
    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [onClose]);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!name.trim()) {
      setError("Name is required.");
      return;
    }

    setIsSaving(true);
    setError(null);

    try {
      const parsedAssigneeId = assigneeId.trim() ? Number(assigneeId) : null;

      if (mode === "create") {
        await onCreate({
          name: name.trim(),
          description: description.trim() || null,
          priority,
          assigneeId: parsedAssigneeId,
        });
      } else {
        await onUpdate({
          name: name.trim(),
          description: description.trim() || null,
          priority,
          status,
          assigneeId: parsedAssigneeId,
        });
      }
      onClose();
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Something went wrong. Try again.",
      );
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 px-4"
      onClick={onClose}
    >
      <div
        onClick={(e) => e.stopPropagation()}
        className="w-full max-w-md rounded-xl border border-slate-200 bg-white p-6 shadow-lg"
      >
        <h2 className="text-base font-semibold text-slate-900">
          {mode === "create" ? "New issue" : "Edit issue"}
        </h2>

        <form onSubmit={handleSubmit} className="mt-5 space-y-4">
          <div>
            <label className={labelStyles} htmlFor="issue-name">
              Name
            </label>
            <input
              id="issue-name"
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              className={inputStyles}
              placeholder="e.g. Fix login redirect loop"
              autoFocus
            />
          </div>

          <div>
            <label className={labelStyles} htmlFor="issue-description">
              Description
            </label>
            <textarea
              id="issue-description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              rows={3}
              className={inputStyles}
              placeholder="Add more detail (optional)"
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className={labelStyles} htmlFor="issue-priority">
                Priority
              </label>
              <select
                id="issue-priority"
                value={priority}
                onChange={(e) => setPriority(e.target.value as Priority)}
                className={inputStyles}
              >
                {PRIORITY_OPTIONS.map((option) => (
                  <option key={option} value={option}>
                    {option}
                  </option>
                ))}
              </select>
            </div>

            {mode === "edit" && (
              <div>
                <label className={labelStyles} htmlFor="issue-status">
                  Status
                </label>
                <select
                  id="issue-status"
                  value={status}
                  onChange={(e) => setStatus(e.target.value as IssueStatus)}
                  className={inputStyles}
                >
                  {STATUS_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </select>
              </div>
            )}
          </div>

          <div>
            <label className={labelStyles} htmlFor="issue-assignee">
              Assignee ID
            </label>
            <input
              id="issue-assignee"
              type="number"
              value={assigneeId}
              onChange={(e) => setAssigneeId(e.target.value)}
              className={inputStyles}
              placeholder="Leave blank if unassigned"
            />
            <p className="mt-1 text-xs text-slate-400">
              Swap this for a proper picker once you have an endpoint to list
              team members.
            </p>
          </div>

          {error && <p className="text-sm text-rose-600">{error}</p>}

          <div className="flex justify-end gap-3 pt-2">
            <button
              type="button"
              onClick={onClose}
              className="rounded-lg px-4 py-2.5 text-sm font-semibold text-slate-600 transition hover:bg-slate-100"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={isSaving}
              className="inline-flex items-center justify-center gap-2 rounded-lg bg-indigo-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {isSaving
                ? "Saving..."
                : mode === "create"
                  ? "Create issue"
                  : "Save changes"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default IssueFormModal;
