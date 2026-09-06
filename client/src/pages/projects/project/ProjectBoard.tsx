import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import type { Assignee, Issue, IssueStatus } from "../../../types/Issue";
import {
  createIssue,
  deleteIssue,
  getIssues,
  updateIssue,
} from "../../../services/issueService";
import { getProjectMembers } from "../../../services/projectService";
import BoardColumn from "../../../components/ui/BoardColumn";
import IssueFormModal from "../../../components/ui/IssueFormModal";

const COLUMNS: { status: IssueStatus; label: string }[] = [
  { status: "Todo", label: "To do" },
  { status: "InProgress", label: "In progress" },
  { status: "InReview", label: "In review" },
  { status: "Done", label: "Done" },
];

function ProjectBoard() {
  const { projectId } = useParams();

  const [issues, setIssues] = useState<Issue[]>([]);
  const [members, setMembers] = useState<Assignee[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);

  const [modalState, setModalState] = useState<
    | { mode: "create"; status: IssueStatus }
    | { mode: "edit"; issue: Issue }
    | null
  >(null);

  useEffect(() => {
    if (!projectId) return;

    let isCancelled = false;

    async function loadBoard() {
      setIsLoading(true);
      setLoadError(null);
      try {
        const [issuesData, membersData] = await Promise.all([
          getIssues(projectId!),
          getProjectMembers(projectId!),
        ]);
        if (!isCancelled) {
          setIssues(issuesData);
          setMembers(membersData);
        }
      } catch (err) {
        if (!isCancelled) {
          setLoadError(
            err instanceof Error ? err.message : "Failed to load board.",
          );
        }
      } finally {
        if (!isCancelled) setIsLoading(false);
      }
    }

    loadBoard();
    return () => {
      isCancelled = true;
    };
  }, [projectId]);

  async function handleDropIssue(issueId: number, status: IssueStatus) {
    if (!projectId) return;

    const issue = issues.find((i) => i.id === issueId);
    if (!issue || issue.status === status) return;

    // Optimistic update, rolled back if the request fails.
    const previousIssues = issues;
    setIssues((current) =>
      current.map((i) => (i.id === issueId ? { ...i, status } : i)),
    );

    try {
      await updateIssue(projectId, issueId, {
        name: issue.name,
        description: issue.description,
        priority: issue.priority,
        status,
        assigneeId: issue.assignee?.id ?? null,
      });
    } catch (err) {
      setIssues(previousIssues);
      setLoadError(
        err instanceof Error ? err.message : "Failed to move issue.",
      );
    }
  }

  async function handleCreateIssue(payload: Parameters<typeof createIssue>[1]) {
    if (!projectId) return;
    const created = await createIssue(projectId, payload);
    setIssues((current) => [...current, created]);
  }

  async function handleUpdateIssue(payload: Parameters<typeof updateIssue>[2]) {
    if (!projectId || modalState?.mode !== "edit") return;
    const updated = await updateIssue(projectId, modalState.issue.id, payload);
    setIssues((current) =>
      current.map((i) => (i.id === updated.id ? updated : i)),
    );
  }

  async function handleDeleteIssue() {
    if (!projectId || modalState?.mode !== "edit") return;
    const issueId = modalState.issue.id;
    await deleteIssue(projectId, issueId);
    setIssues((current) => current.filter((i) => i.id !== issueId));
  }

  return (
    <main className="min-w-0 flex-1">
      <div className="px-6 py-8 lg:px-10">
        <div className="mb-6">
          <h1 className="text-2xl font-bold tracking-tight text-slate-900">
            Board
          </h1>
          <p className="mt-1 text-sm text-slate-500">
            Drag a card between columns to update its status.
          </p>
        </div>

        {loadError && (
          <div className="mb-6 rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
            {loadError}
          </div>
        )}

        {isLoading ? (
          <p className="text-sm text-slate-500">Loading issues...</p>
        ) : (
          <div className="flex gap-4 overflow-x-auto pb-4">
            {COLUMNS.map((column) => (
              <BoardColumn
                key={column.status}
                status={column.status}
                label={column.label}
                issues={issues.filter((i) => i.status === column.status)}
                onIssueClick={(issue) => setModalState({ mode: "edit", issue })}
                onDropIssue={handleDropIssue}
                onAddIssue={(status) =>
                  setModalState({ mode: "create", status })
                }
              />
            ))}
          </div>
        )}
      </div>

      {modalState?.mode === "create" && (
        <IssueFormModal
          mode="create"
          initialStatus={modalState.status}
          members={members}
          onClose={() => setModalState(null)}
          onCreate={handleCreateIssue}
          onUpdate={handleUpdateIssue}
        />
      )}

      {modalState?.mode === "edit" && (
        <IssueFormModal
          mode="edit"
          initialStatus={modalState.issue.status}
          issue={modalState.issue}
          members={members}
          onClose={() => setModalState(null)}
          onCreate={handleCreateIssue}
          onUpdate={handleUpdateIssue}
          onDelete={handleDeleteIssue}
        />
      )}
    </main>
  );
}

export default ProjectBoard;
