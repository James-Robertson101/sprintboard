import { useState } from "react";

import type {
  Sprint,
  CreateSprintPayload,
  UpdateSprintPayload,
} from "../../../types/Sprint";

interface SprintModalProps {
  sprint?: Sprint | null;
  onClose: () => void;
  onSubmit: (
    payload: CreateSprintPayload | UpdateSprintPayload,
  ) => Promise<void>;
}

function toDateInputValue(date: string) {
  return date ? date.slice(0, 10) : "";
}

export default function SprintModal({
  sprint,
  onClose,
  onSubmit,
}: SprintModalProps) {
  const isEditing = !!sprint;

  const [name, setName] = useState(sprint?.name ?? "");
  const [goal, setGoal] = useState(sprint?.goal ?? "");

  const [startDate, setStartDate] = useState(
    sprint ? toDateInputValue(sprint.startDate) : "",
  );

  const [endDate, setEndDate] = useState(
    sprint ? toDateInputValue(sprint.endDate) : "",
  );

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();

    if (!name.trim()) {
      setError("Sprint name is required.");
      return;
    }

    if (!startDate || !endDate) {
      setError("Start and end dates are required.");
      return;
    }

    if (endDate <= startDate) {
      setError("End date must be after the start date.");
      return;
    }

    try {
      setSubmitting(true);
      setError(null);

      await onSubmit({
        name: name.trim(),
        goal: goal.trim() || null,
        startDate,
        endDate,
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to save sprint.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4"
      onMouseDown={(e) => {
        if (e.target === e.currentTarget) {
          onClose();
        }
      }}
    >
      <div className="w-full max-w-lg rounded-lg bg-white shadow-xl">
        <div className="border-b px-6 py-4">
          <h2 className="text-lg font-semibold">
            {isEditing ? "Edit Sprint" : "Create Sprint"}
          </h2>

          <p className="mt-1 text-sm text-gray-500">
            {isEditing
              ? "Update the sprint details."
              : "Create a sprint for this project."}
          </p>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="space-y-4 px-6 py-5">
            {error && (
              <div className="rounded-md bg-red-50 px-4 py-3 text-sm text-red-700">
                {error}
              </div>
            )}

            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">
                Name
              </label>

              <input
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="Sprint 4"
                className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
                disabled={submitting}
              />
            </div>

            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">
                Goal
              </label>

              <textarea
                value={goal}
                onChange={(e) => setGoal(e.target.value)}
                placeholder="What do you want to accomplish?"
                rows={3}
                className="w-full resize-none rounded-md border border-gray-300 px-3 py-2 text-sm outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
                disabled={submitting}
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="mb-1 block text-sm font-medium text-gray-700">
                  Start date
                </label>

                <input
                  type="date"
                  value={startDate}
                  onChange={(e) => setStartDate(e.target.value)}
                  className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
                  disabled={submitting}
                />
              </div>

              <div>
                <label className="mb-1 block text-sm font-medium text-gray-700">
                  End date
                </label>

                <input
                  type="date"
                  value={endDate}
                  onChange={(e) => setEndDate(e.target.value)}
                  className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
                  disabled={submitting}
                />
              </div>
            </div>
          </div>

          <div className="flex justify-end gap-3 border-t bg-gray-50 px-6 py-4">
            <button
              type="button"
              onClick={onClose}
              disabled={submitting}
              className="rounded-md border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-100 disabled:opacity-50"
            >
              Cancel
            </button>

            <button
              type="submit"
              disabled={submitting}
              className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
            >
              {submitting
                ? "Saving..."
                : isEditing
                  ? "Save Changes"
                  : "Create Sprint"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
