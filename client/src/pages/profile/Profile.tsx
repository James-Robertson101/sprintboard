import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../context/useAuth";
import AvatarModal from "../../components/auth/AvatarModal"; // adjust path to match your project
import {
  deleteMyAccount,
  updateProfile,
} from "../../services/userProfileService";

function Profile() {
  const { user, loading, refetchUser, logout } = useAuth();
  const navigate = useNavigate();

  const [name, setName] = useState(user?.name ?? "");
  const [avatarUrl, setAvatarUrl] = useState(user?.avatarUrl ?? "");
  const [isAvatarModalOpen, setIsAvatarModalOpen] = useState(false);

  const [isSaving, setIsSaving] = useState(false);
  const [saveError, setSaveError] = useState<string | null>(null);
  const [saveSuccess, setSaveSuccess] = useState(false);

  const [isConfirmingDelete, setIsConfirmingDelete] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  async function handleSave() {
    setIsSaving(true);
    setSaveError(null);
    setSaveSuccess(false);

    try {
      await updateProfile({
        name: name.trim(),
        avatarUrl: avatarUrl || null,
      });

      await refetchUser();
      setSaveSuccess(true);
    } catch (error) {
      setSaveError(
        error instanceof Error
          ? error.message
          : "Something went wrong saving your profile.",
      );
    } finally {
      setIsSaving(false);
    }
  }

  async function handleDelete() {
    setIsDeleting(true);
    setDeleteError(null);

    try {
      await deleteMyAccount();
      await logout();
      navigate("/login", { replace: true });
    } catch (error) {
      // Sole-owner conflict (409) and any other failure land here,
      // surfaced inline rather than losing the user to a redirect.
      setDeleteError(
        error instanceof Error
          ? error.message
          : "Something went wrong deleting your account.",
      );
      setIsConfirmingDelete(false);
      setIsDeleting(false);
    }
  }

  if (loading) {
    return (
      <div className="mx-auto max-w-2xl px-6 py-10 text-sm text-muted">
        Loading profile…
      </div>
    );
  }

  if (!user) {
    return (
      <div className="mx-auto max-w-2xl px-6 py-10">
        <p className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          You need to be signed in to view this page.
        </p>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-2xl space-y-8 px-6 py-10">
      {/* Profile settings */}
      <section className="rounded-2xl border border-border bg-surface p-6">
        <h2 className="text-lg font-semibold text-text">Profile</h2>
        <p className="mt-1 text-sm text-muted">Update your name and avatar.</p>

        <div className="mt-6 flex items-center gap-4">
          <div className="flex h-16 w-16 shrink-0 items-center justify-center overflow-hidden rounded-full bg-indigo-100 text-lg font-semibold text-indigo-600">
            {avatarUrl ? (
              <img
                src={avatarUrl}
                alt={`${name || "User"} avatar`}
                className="h-full w-full object-cover"
              />
            ) : (
              <span>{name.charAt(0).toUpperCase() || "?"}</span>
            )}
          </div>

          <button
            type="button"
            onClick={() => setIsAvatarModalOpen(true)}
            className="rounded-md border border-border px-3 py-2 text-sm font-medium text-text transition hover:bg-slate-50"
          >
            Change avatar
          </button>
        </div>

        <div className="mt-6">
          <label
            htmlFor="profile-name"
            className="block text-sm font-medium text-text"
          >
            Name
          </label>
          <input
            id="profile-name"
            type="text"
            value={name}
            onChange={(event) => setName(event.target.value)}
            className="mt-1 w-full rounded-md border border-border px-3 py-2 text-sm text-text focus:border-primary focus:outline-none"
          />
        </div>

        <div className="mt-4">
          <label className="block text-sm font-medium text-text">Email</label>
          <p className="mt-1 text-sm text-muted">{user.email}</p>
        </div>

        {saveError && (
          <p className="mt-4 rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">
            {saveError}
          </p>
        )}

        {saveSuccess && !saveError && (
          <p className="mt-4 rounded-lg border border-green-200 bg-green-50 px-3 py-2 text-sm text-green-700">
            Profile updated.
          </p>
        )}

        <div className="mt-6 flex justify-end">
          <button
            type="button"
            onClick={handleSave}
            disabled={isSaving || !name.trim()}
            className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-white transition hover:bg-primary-hover disabled:cursor-not-allowed disabled:opacity-50"
          >
            {isSaving ? "Saving…" : "Save changes"}
          </button>
        </div>
      </section>

      {/* Danger zone */}
      <section className="rounded-2xl border border-red-200 bg-red-50/40 p-6">
        <h2 className="text-lg font-semibold text-red-700">Danger zone</h2>
        <p className="mt-1 text-sm text-red-700/80">
          Deleting your account is permanent and cannot be undone.
        </p>

        {deleteError && (
          <p className="mt-4 rounded-lg border border-red-300 bg-red-100 px-3 py-2 text-sm text-red-800">
            {deleteError}
          </p>
        )}

        {!isConfirmingDelete ? (
          <div className="mt-4">
            <button
              type="button"
              onClick={() => {
                setDeleteError(null);
                setIsConfirmingDelete(true);
              }}
              className="rounded-md border border-red-300 px-4 py-2 text-sm font-medium text-red-700 transition hover:bg-red-100"
            >
              Delete my account
            </button>
          </div>
        ) : (
          <div className="mt-4 rounded-lg border border-red-300 bg-white p-4">
            <p className="text-sm text-text">
              Are you sure? This cannot be undone.
            </p>
            <div className="mt-3 flex gap-3">
              <button
                type="button"
                onClick={() => setIsConfirmingDelete(false)}
                disabled={isDeleting}
                className="rounded-md border border-border px-4 py-2 text-sm font-medium text-text transition hover:bg-slate-50 disabled:opacity-50"
              >
                Cancel
              </button>
              <button
                type="button"
                onClick={handleDelete}
                disabled={isDeleting}
                className="rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-red-700 disabled:cursor-not-allowed disabled:opacity-50"
              >
                {isDeleting ? "Deleting…" : "Yes, delete my account"}
              </button>
            </div>
          </div>
        )}
      </section>

      {isAvatarModalOpen && (
        <AvatarModal
          name={name}
          selectedAvatarUrl={avatarUrl}
          onSelect={setAvatarUrl}
          onClose={() => setIsAvatarModalOpen(false)}
        />
      )}
    </div>
  );
}

export default Profile;
