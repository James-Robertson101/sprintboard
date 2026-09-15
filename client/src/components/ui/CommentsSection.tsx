import { useEffect, useState } from "react";
import type { Comment } from "../../types/Comment";
import {
  createComment,
  deleteComment,
  getComments,
  updateComment,
} from "../../services/commentService";

interface CommentsSectionProps {
  projectId: string;
  issueId: number;
}

function initials(name: string) {
  return name
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase())
    .join("");
}

function formatDate(date: string) {
  return new Date(date).toLocaleString(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  });
}

function CommentsSection({ projectId, issueId }: CommentsSectionProps) {
  const [comments, setComments] = useState<Comment[]>([]);
  const [content, setContent] = useState("");

  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [editingId, setEditingId] = useState<number | null>(null);
  const [editingContent, setEditingContent] = useState("");

  useEffect(() => {
    let isCancelled = false;

    async function loadComments() {
      setIsLoading(true);
      setError(null);

      try {
        const data = await getComments(projectId, issueId);

        if (!isCancelled) {
          setComments(data);
        }
      } catch (err) {
        if (!isCancelled) {
          setError(
            err instanceof Error ? err.message : "Failed to load comments.",
          );
        }
      } finally {
        if (!isCancelled) {
          setIsLoading(false);
        }
      }
    }

    loadComments();

    return () => {
      isCancelled = true;
    };
  }, [projectId, issueId]);

  async function handleAddComment(e: React.FormEvent) {
    e.preventDefault();

    const trimmed = content.trim();

    if (!trimmed) return;

    setIsSubmitting(true);
    setError(null);

    try {
      const created = await createComment(projectId, issueId, {
        content: trimmed,
      });

      setComments((current) => [...current, created]);
      setContent("");
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to create comment.",
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  function startEditing(comment: Comment) {
    setEditingId(comment.id);
    setEditingContent(comment.content);
    setError(null);
  }

  function cancelEditing() {
    setEditingId(null);
    setEditingContent("");
  }

  async function handleUpdateComment(commentId: number) {
    const trimmed = editingContent.trim();

    if (!trimmed) return;

    setError(null);

    try {
      const updated = await updateComment(projectId, issueId, commentId, {
        content: trimmed,
      });

      setComments((current) =>
        current.map((comment) =>
          comment.id === updated.id ? updated : comment,
        ),
      );

      cancelEditing();
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to update comment.",
      );
    }
  }

  async function handleDeleteComment(commentId: number) {
    setError(null);

    try {
      await deleteComment(projectId, issueId, commentId);

      setComments((current) =>
        current.filter((comment) => comment.id !== commentId),
      );
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to delete comment.",
      );
    }
  }

  return (
    <section className="mt-6 border-t border-slate-200 pt-6">
      <div className="flex items-center justify-between">
        <h3 className="text-sm font-semibold text-slate-900">Comments</h3>

        <span className="rounded-full bg-slate-100 px-2 py-0.5 text-xs font-medium text-slate-500">
          {comments.length}
        </span>
      </div>

      {error && (
        <div className="mt-3 rounded-lg border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">
          {error}
        </div>
      )}

      {isLoading ? (
        <p className="mt-4 text-sm text-slate-500">Loading comments...</p>
      ) : comments.length === 0 ? (
        <p className="mt-4 text-sm text-slate-500">No comments yet.</p>
      ) : (
        <div className="mt-4 space-y-4">
          {comments.map((comment) => (
            <div key={comment.id} className="flex gap-3">
              {comment.author.avatarUrl ? (
                <img
                  src={comment.author.avatarUrl}
                  alt={comment.author.name}
                  className="h-8 w-8 shrink-0 rounded-full object-cover"
                />
              ) : (
                <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-indigo-100 text-xs font-semibold text-indigo-700">
                  {initials(comment.author.name)}
                </div>
              )}

              <div className="min-w-0 flex-1">
                <div className="flex flex-wrap items-baseline gap-x-2 gap-y-0.5">
                  <span className="text-sm font-medium text-slate-900">
                    {comment.author.name}
                  </span>

                  <span className="text-xs text-slate-400">
                    {formatDate(comment.createdAt)}
                  </span>

                  {comment.updatedAt && (
                    <span className="text-xs text-slate-400">(edited)</span>
                  )}
                </div>

                {editingId === comment.id ? (
                  <div className="mt-2">
                    <textarea
                      value={editingContent}
                      onChange={(e) => setEditingContent(e.target.value)}
                      rows={3}
                      className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-900 shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                    />

                    <div className="mt-2 flex gap-2">
                      <button
                        type="button"
                        onClick={() => handleUpdateComment(comment.id)}
                        disabled={!editingContent.trim()}
                        className="rounded-lg bg-indigo-600 px-3 py-1.5 text-xs font-semibold text-white transition hover:bg-indigo-700 disabled:cursor-not-allowed disabled:opacity-50"
                      >
                        Save
                      </button>

                      <button
                        type="button"
                        onClick={cancelEditing}
                        className="rounded-lg px-3 py-1.5 text-xs font-semibold text-slate-600 transition hover:bg-slate-100"
                      >
                        Cancel
                      </button>
                    </div>
                  </div>
                ) : (
                  <>
                    <p className="mt-1 whitespace-pre-wrap text-sm leading-relaxed text-slate-600">
                      {comment.content}
                    </p>

                    <div className="mt-1.5 flex gap-3">
                      <button
                        type="button"
                        onClick={() => startEditing(comment)}
                        className="text-xs font-medium text-slate-400 transition hover:text-slate-600"
                      >
                        Edit
                      </button>

                      <button
                        type="button"
                        onClick={() => handleDeleteComment(comment.id)}
                        className="text-xs font-medium text-rose-400 transition hover:text-rose-600"
                      >
                        Delete
                      </button>
                    </div>
                  </>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      <form onSubmit={handleAddComment} className="mt-5">
        <textarea
          value={content}
          onChange={(e) => setContent(e.target.value)}
          rows={3}
          placeholder="Write a comment..."
          className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-900 shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
        />

        <div className="mt-2 flex justify-end">
          <button
            type="submit"
            disabled={isSubmitting || !content.trim()}
            className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-semibold text-white shadow-sm transition hover:bg-indigo-700 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {isSubmitting ? "Posting..." : "Add comment"}
          </button>
        </div>
      </form>
    </section>
  );
}

export default CommentsSection;
