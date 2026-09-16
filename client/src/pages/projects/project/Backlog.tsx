import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import type { Issue } from "../../../types/Issue";
import {
  getBacklog,
  assignIssueToSprint,
} from "../../../services/issueService";
import { getSprints } from "../../../services/sprintService"; // adjust if named differently

interface SprintOption {
  id: number;
  name: string;
}

function Backlog() {
  const { projectId } = useParams();

  const [issues, setIssues] = useState<Issue[]>([]);
  const [sprints, setSprints] = useState<SprintOption[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [updatingId, setUpdatingId] = useState<number | null>(null);

  useEffect(() => {
    if (!projectId) return;

    let isCancelled = false;

    async function load() {
      setIsLoading(true);
      setLoadError(null);
      try {
        const [backlogData, sprintData] = await Promise.all([
          getBacklog(projectId!),
          getSprints(projectId!),
        ]);
        if (!isCancelled) {
          setIssues(backlogData);
          setSprints(sprintData);
        }
      } catch (err) {
        if (!isCancelled) {
          setLoadError(
            err instanceof Error ? err.message : "Failed to load backlog.",
          );
        }
      } finally {
        if (!isCancelled) setIsLoading(false);
      }
    }

    load();
    return () => {
      isCancelled = true;
    };
  }, [projectId]);

  async function handleAssignSprint(issueId: number, sprintId: number | null) {
    if (!projectId) return;

    const previousIssues = issues;
    setUpdatingId(issueId);

    // Optimistic update: issue leaves the backlog once it's assigned to a sprint.
    setIssues((current) =>
      sprintId === null ? current : current.filter((i) => i.id !== issueId),
    );

    try {
      await assignIssueToSprint(projectId, issueId, sprintId);
    } catch (err) {
      setIssues(previousIssues);
      setLoadError(
        err instanceof Error ? err.message : "Failed to move issue.",
      );
    } finally {
      setUpdatingId(null);
    }
  }

  return (
    <main className="min-w-0 flex-1">
      <div className="px-6 py-8 lg:px-10">
        <div className="mb-6">
          <h1 className="text-2xl font-bold tracking-tight text-slate-900">
            Backlog
          </h1>
          <p className="mt-1 text-sm text-slate-500">
            Issues not yet assigned to a sprint.
          </p>
        </div>

        {loadError && (
          <div className="mb-6 rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
            {loadError}
          </div>
        )}

        {isLoading ? (
          <p className="text-sm text-slate-500">Loading backlog...</p>
        ) : issues.length === 0 ? (
          <p className="text-sm text-slate-500">The backlog is empty.</p>
        ) : (
          <ul className="divide-y divide-slate-200 rounded-lg border border-slate-200 bg-white">
            {issues.map((issue) => (
              <li
                key={issue.id}
                className="flex items-center justify-between gap-4 px-4 py-3"
              >
                <div className="min-w-0">
                  <p className="truncate text-sm font-medium text-slate-900">
                    {issue.name}
                  </p>
                  {issue.description && (
                    <p className="truncate text-xs text-slate-500">
                      {issue.description}
                    </p>
                  )}
                </div>

                <select
                  className="shrink-0 rounded-md border border-slate-300 px-2 py-1 text-sm text-slate-700"
                  disabled={updatingId === issue.id}
                  value=""
                  onChange={(e) => {
                    const value = e.target.value;
                    if (value) handleAssignSprint(issue.id, Number(value));
                  }}
                >
                  <option value="" disabled>
                    Add to sprint...
                  </option>
                  {sprints.map((sprint) => (
                    <option key={sprint.id} value={sprint.id}>
                      {sprint.name}
                    </option>
                  ))}
                </select>
              </li>
            ))}
          </ul>
        )}
      </div>
    </main>
  );
}

export default Backlog;
