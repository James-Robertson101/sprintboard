import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

import type {
  Sprint,
  CreateSprintPayload,
  UpdateSprintPayload,
} from "../../../types/Sprint";

import {
  completeSprint,
  createSprint,
  deleteSprint,
  getSprints,
  startSprint,
  updateSprint,
} from "../../../services/sprintService";

import SprintModal from "./SprintModal";

export default function Sprints() {
  const { projectId } = useParams<{ projectId: string }>();

  const [sprints, setSprints] = useState<Sprint[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [showModal, setShowModal] = useState(false);
  const [editingSprint, setEditingSprint] = useState<Sprint | null>(null);

  const [actionLoading, setActionLoading] = useState<number | null>(null);

  useEffect(() => {
    if (!projectId) return;

    const currentProjectId = projectId;

    let cancelled = false;

    async function load() {
      try {
        setLoading(true);
        setError(null);

        const data = await getSprints(currentProjectId);

        if (!cancelled) {
          setSprints(data);
        }
      } catch (err) {
        if (!cancelled) {
          setError(
            err instanceof Error ? err.message : "Failed to load sprints.",
          );
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    load();

    return () => {
      cancelled = true;
    };
  }, [projectId]);

  async function refreshSprints() {
    if (!projectId) return;

    try {
      setError(null);

      const data = await getSprints(projectId);
      setSprints(data);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to refresh sprints.",
      );
    }
  }

  function handleCreateClick() {
    setEditingSprint(null);
    setShowModal(true);
  }

  function handleEditClick(sprint: Sprint) {
    setEditingSprint(sprint);
    setShowModal(true);
  }

  async function handleSubmit(
    payload: CreateSprintPayload | UpdateSprintPayload,
  ) {
    if (!projectId) return;

    if (editingSprint) {
      await updateSprint(projectId, editingSprint.id, payload);
    } else {
      await createSprint(projectId, payload);
    }

    setShowModal(false);
    setEditingSprint(null);

    await refreshSprints();
  }

  async function handleStart(sprint: Sprint) {
    if (!projectId) return;

    try {
      setActionLoading(sprint.id);
      setError(null);

      await startSprint(projectId, sprint.id);
      await refreshSprints();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to start sprint.");
    } finally {
      setActionLoading(null);
    }
  }

  async function handleComplete(sprint: Sprint) {
    if (!projectId) return;

    const confirmed = window.confirm(
      `Are you sure you want to complete "${sprint.name}"?`,
    );

    if (!confirmed) return;

    try {
      setActionLoading(sprint.id);
      setError(null);

      await completeSprint(projectId, sprint.id);
      await refreshSprints();
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to complete sprint.",
      );
    } finally {
      setActionLoading(null);
    }
  }

  async function handleDelete(sprint: Sprint) {
    if (!projectId) return;

    const confirmed = window.confirm(
      `Are you sure you want to delete "${sprint.name}"?`,
    );

    if (!confirmed) return;

    try {
      setActionLoading(sprint.id);
      setError(null);

      await deleteSprint(projectId, sprint.id);
      await refreshSprints();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to delete sprint.");
    } finally {
      setActionLoading(null);
    }
  }

  if (loading) {
    return <div className="p-6">Loading sprints...</div>;
  }

  return (
    <div className="space-y-6 p-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Sprints</h1>
          <p className="mt-1 text-sm text-gray-500">
            Plan and manage your project sprints.
          </p>
        </div>

        <button
          onClick={handleCreateClick}
          className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
        >
          Create Sprint
        </button>
      </div>

      {/* Error */}
      {error && (
        <div className="flex items-center justify-between rounded-md bg-red-50 px-4 py-3 text-sm text-red-700">
          <span>{error}</span>

          <button
            onClick={() => setError(null)}
            className="font-medium hover:underline"
          >
            Dismiss
          </button>
        </div>
      )}

      {/* Empty state */}
      {sprints.length === 0 ? (
        <div className="rounded-lg border bg-white p-10 text-center">
          <h2 className="font-medium">No sprints yet</h2>

          <p className="mt-1 text-sm text-gray-500">
            Create your first sprint to start planning work.
          </p>

          <button
            onClick={handleCreateClick}
            className="mt-4 rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
          >
            Create Sprint
          </button>
        </div>
      ) : (
        <div className="space-y-4">
          {sprints.map((sprint) => (
            <SprintCard
              key={sprint.id}
              sprint={sprint}
              actionLoading={actionLoading === sprint.id}
              onEdit={() => handleEditClick(sprint)}
              onStart={() => handleStart(sprint)}
              onComplete={() => handleComplete(sprint)}
              onDelete={() => handleDelete(sprint)}
            />
          ))}
        </div>
      )}

      {/* Modal */}
      {showModal && (
        <SprintModal
          key={editingSprint?.id ?? "create"}
          sprint={editingSprint}
          onClose={() => {
            setShowModal(false);
            setEditingSprint(null);
          }}
          onSubmit={handleSubmit}
        />
      )}
    </div>
  );
}

interface SprintCardProps {
  sprint: Sprint;
  actionLoading: boolean;
  onEdit: () => void;
  onStart: () => void;
  onComplete: () => void;
  onDelete: () => void;
}

function SprintCard({
  sprint,
  actionLoading,
  onEdit,
  onStart,
  onComplete,
  onDelete,
}: SprintCardProps) {
  return (
    <div className="rounded-lg border bg-white p-5 shadow-sm">
      <div className="flex items-start justify-between gap-4">
        <div>
          <div className="flex items-center gap-3">
            <h2 className="text-lg font-semibold">{sprint.name}</h2>

            <SprintStatusBadge status={sprint.status} />
          </div>

          {sprint.goal && (
            <p className="mt-1 text-sm text-gray-500">{sprint.goal}</p>
          )}
        </div>

        {sprint.status !== "Completed" && (
          <button
            onClick={onEdit}
            disabled={actionLoading}
            className="text-sm font-medium text-gray-600 hover:text-gray-900 disabled:opacity-50"
          >
            Edit
          </button>
        )}
      </div>

      <div className="mt-5 flex flex-wrap gap-x-6 gap-y-2 text-sm text-gray-500">
        <span>
          {formatDate(sprint.startDate)} → {formatDate(sprint.endDate)}
        </span>

        <span>
          {sprint.issueCount} {sprint.issueCount === 1 ? "issue" : "issues"}
        </span>
      </div>

      <div className="mt-5 flex gap-2 border-t pt-4">
        {sprint.status === "Planned" && (
          <>
            <button
              onClick={onStart}
              disabled={actionLoading}
              className="rounded-md bg-blue-600 px-3 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
            >
              {actionLoading ? "Starting..." : "Start Sprint"}
            </button>

            <button
              onClick={onDelete}
              disabled={actionLoading}
              className="rounded-md border border-red-300 px-3 py-2 text-sm font-medium text-red-600 hover:bg-red-50 disabled:opacity-50"
            >
              Delete
            </button>
          </>
        )}

        {sprint.status === "Active" && (
          <button
            onClick={onComplete}
            disabled={actionLoading}
            className="rounded-md bg-green-600 px-3 py-2 text-sm font-medium text-white hover:bg-green-700 disabled:opacity-50"
          >
            {actionLoading ? "Completing..." : "Complete Sprint"}
          </button>
        )}
      </div>
    </div>
  );
}

function SprintStatusBadge({ status }: { status: Sprint["status"] }) {
  const styles = {
    Planned: "bg-gray-100 text-gray-700",
    Active: "bg-blue-100 text-blue-700",
    Completed: "bg-green-100 text-green-700",
  };

  return (
    <span
      className={`rounded-full px-3 py-1 text-xs font-medium ${styles[status]}`}
    >
      {status}
    </span>
  );
}

function formatDate(date: string) {
  return new Date(date).toLocaleDateString();
}
