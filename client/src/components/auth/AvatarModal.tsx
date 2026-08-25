import { useMemo, useState } from "react";
import type { AvatarCategory, AvatarOption } from "../../types/avatar";

type AvatarModalProps = {
  name: string;
  selectedAvatarUrl: string;
  onSelect: (avatarUrl: string) => void;
  onClose: () => void;
};

const DICEBEAR_BASE_URL = "https://api.dicebear.com/10.x";

function createAvatarUrl(style: string, seed: string) {
  const params = new URLSearchParams({
    seed,
    animationVariant: "medium",
  });

  return `${DICEBEAR_BASE_URL}/${style}/svg?${params.toString()}`;
}

function AvatarModal({
  name,
  selectedAvatarUrl,
  onSelect,
  onClose,
}: AvatarModalProps) {
  const [temporarySelection, setTemporarySelection] =
    useState(selectedAvatarUrl);

  const avatarCategories = useMemo<AvatarCategory[]>(() => {
    const nameSeed = name.trim() || "User";

    return [
      {
        id: "blobs",
        name: "Blobs",
        options: Array.from({ length: 10 }, (_, index): AvatarOption => {
          const seed = `blobs-${index + 1}`;

          return {
            id: seed,
            name: `Blob ${index + 1}`,
            url: createAvatarUrl("blobs", seed),
          };
        }),
      },

      {
        id: "glass",
        name: "Glass",
        options: Array.from({ length: 10 }, (_, index): AvatarOption => {
          const seed = `glass-${index + 1}`;

          return {
            id: seed,
            name: `Glass ${index + 1}`,
            url: createAvatarUrl("glass", seed),
          };
        }),
      },

      {
        id: "initial-face",
        name: "Initial Face",
        options: Array.from({ length: 10 }, (_, index): AvatarOption => {
          const seed = `${nameSeed}-${index + 1}`;

          return {
            id: `initial-face-${index + 1}`,
            name: `Initial Face ${index + 1}`,
            url: createAvatarUrl("initial-face", seed),
          };
        }),
      },
    ];
  }, [name]);

  function handleUseAvatar() {
    if (!temporarySelection) {
      return;
    }

    onSelect(temporarySelection);
    onClose();
  }

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4"
      role="dialog"
      aria-modal="true"
      aria-labelledby="avatar-modal-title"
      onMouseDown={(event) => {
        if (event.target === event.currentTarget) {
          onClose();
        }
      }}
    >
      <div className="max-h-[90vh] w-full max-w-3xl overflow-hidden rounded-2xl bg-surface shadow-xl">
        {/* Header */}
        <div className="flex items-center justify-between border-b border-border px-6 py-4">
          <div>
            <h2
              id="avatar-modal-title"
              className="text-lg font-semibold text-text"
            >
              Choose your avatar
            </h2>

            <p className="mt-1 text-sm text-muted">
              Pick an avatar for your SprintBoard profile.
            </p>
          </div>

          <button
            type="button"
            onClick={onClose}
            className="rounded-lg p-2 text-muted transition hover:bg-slate-100 hover:text-text"
            aria-label="Close avatar picker"
          >
            <svg
              className="h-5 w-5"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth={2}
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M6 6l12 12M18 6L6 18"
              />
            </svg>
          </button>
        </div>

        {/* Avatar options */}
        <div className="max-h-[65vh] space-y-8 overflow-y-auto p-6">
          {avatarCategories.map((category) => (
            <section key={category.id}>
              <h3 className="mb-3 text-sm font-semibold text-text">
                {category.name}
              </h3>

              <div className="grid grid-cols-5 gap-3 md:grid-cols-10">
                {category.options.map((avatar) => {
                  const isSelected = temporarySelection === avatar.url;

                  return (
                    <button
                      key={avatar.id}
                      type="button"
                      title={avatar.name}
                      onClick={() => setTemporarySelection(avatar.url)}
                      className={`group relative aspect-square overflow-hidden rounded-xl border-2 bg-surface p-1 transition ${
                        isSelected
                          ? "border-primary ring-2 ring-primary/20"
                          : "border-border hover:border-primary/50"
                      }`}
                    >
                      <img
                        src={avatar.url}
                        alt={avatar.name}
                        className="h-full w-full rounded-lg object-cover"
                      />

                      {isSelected && (
                        <span className="absolute right-1 top-1 flex h-5 w-5 items-center justify-center rounded-full bg-primary text-white shadow-sm">
                          <svg
                            className="h-3 w-3"
                            viewBox="0 0 20 20"
                            fill="currentColor"
                          >
                            <path
                              fillRule="evenodd"
                              d="M16.704 5.29a1 1 0 010 1.414l-7.25 7.25a1 1 0 01-1.414 0l-3.25-3.25a1 1 0 111.414-1.414l6.543-6.543a1 1 0 011.414 0z"
                              clipRule="evenodd"
                            />
                          </svg>
                        </span>
                      )}
                    </button>
                  );
                })}
              </div>
            </section>
          ))}
        </div>

        {/* Footer */}
        <div className="flex justify-end gap-3 border-t border-border px-6 py-4">
          <button
            type="button"
            onClick={onClose}
            className="rounded-md border border-border px-4 py-2 text-sm font-medium text-text transition hover:bg-slate-50"
          >
            Cancel
          </button>

          <button
            type="button"
            onClick={handleUseAvatar}
            disabled={!temporarySelection}
            className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-white transition hover:bg-primary-hover disabled:cursor-not-allowed disabled:opacity-50"
          >
            Use this avatar
          </button>
        </div>
      </div>
    </div>
  );
}

export default AvatarModal;
