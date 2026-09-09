import { useEffect, useState } from "react";
import {
  getProjectMembers,
  removeProjectMember,
  getAvailableUsers,
  addProjectMember,
} from "../../../../services/projectService";
import type { ProjectMember, UserSummary } from "../../../../types/project";
import { MemberRow } from "./MemberRow";
import { UserSearchResultRow } from "./UserSearchResultRow";
import { EmptyMembersIcon } from "./icons";

interface MembersSettingsPanelProps {
  projectId: string;
}

export function MembersSettingsPanel({ projectId }: MembersSettingsPanelProps) {
  const [members, setMembers] = useState<ProjectMember[]>([]);
  const [isLoadingMembers, setIsLoadingMembers] = useState(false);
  const [membersError, setMembersError] = useState<string | null>(null);

  const [searchQuery, setSearchQuery] = useState("");
  const [searchResults, setSearchResults] = useState<UserSummary[]>([]);
  const [isSearching, setIsSearching] = useState(false);
  const [addingUserId, setAddingUserId] = useState<number | null>(null);

  useEffect(() => {
    async function loadMembers() {
      setIsLoadingMembers(true);
      setMembersError(null);

      try {
        const data = await getProjectMembers(projectId);
        setMembers(data);
      } catch (error) {
        setMembersError(
          error instanceof Error
            ? error.message
            : "Failed to load project members.",
        );
      } finally {
        setIsLoadingMembers(false);
      }
    }

    loadMembers();
  }, [projectId]);

  async function handleSearch(e: React.SubmitEvent<HTMLFormElement>) {
    e.preventDefault();
    if (!searchQuery.trim()) return;

    setIsSearching(true);
    setMembersError(null);

    try {
      const results = await getAvailableUsers(projectId, searchQuery.trim());
      setSearchResults(results);
    } catch (error) {
      setMembersError(
        error instanceof Error ? error.message : "Failed to search users.",
      );
    } finally {
      setIsSearching(false);
    }
  }

  async function handleAddMember(user: UserSummary) {
    setAddingUserId(user.id);
    setMembersError(null);

    try {
      const member = await addProjectMember(projectId, user.id);
      setMembers((current) => [...current, member]);
      setSearchResults((current) => current.filter((u) => u.id !== user.id));
    } catch (error) {
      setMembersError(
        error instanceof Error
          ? error.message
          : "Failed to add project member.",
      );
    } finally {
      setAddingUserId(null);
    }
  }

  async function handleRemoveMember(memberId: number) {
    const confirmed = window.confirm(
      "Are you sure you want to remove this member from the project?",
    );
    if (!confirmed) return;

    try {
      await removeProjectMember(Number(projectId), memberId);
      setMembers((current) =>
        current.filter((member) => member.id !== memberId),
      );
    } catch (error) {
      setMembersError(
        error instanceof Error
          ? error.message
          : "Failed to remove project member.",
      );
    }
  }

  return (
    <div className="space-y-6">
      {/* Add member */}
      <div className="rounded-xl border border-slate-200 bg-white shadow-sm">
        <div className="border-b border-slate-200 px-6 py-5">
          <h2 className="text-base font-semibold text-slate-900">
            Add a member
          </h2>
          <p className="mt-1 text-sm text-slate-500">
            Search for someone by name or email to add them to this project.
          </p>
        </div>

        <form onSubmit={handleSearch} className="px-6 py-6">
          <div className="flex flex-col gap-3 sm:flex-row">
            <div className="min-w-0 flex-1">
              <label htmlFor="member-search" className="sr-only">
                Search users
              </label>
              <input
                id="member-search"
                type="text"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                placeholder="Search by name or email"
                className="block w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm text-slate-900 outline-none transition placeholder:text-slate-400 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
              />
            </div>

            <button
              type="submit"
              disabled={isSearching || !searchQuery.trim()}
              className="inline-flex items-center justify-center rounded-lg bg-indigo-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-indigo-700 disabled:cursor-not-allowed disabled:opacity-50"
            >
              {isSearching ? "Searching..." : "Search"}
            </button>
          </div>
        </form>

        {searchResults.length > 0 && (
          <div className="border-t border-slate-200 px-6 py-4">
            <p className="mb-3 text-xs font-medium uppercase tracking-wide text-slate-500">
              Search results
            </p>
            <div className="divide-y divide-slate-200 rounded-lg border border-slate-200">
              {searchResults.map((user) => (
                <UserSearchResultRow
                  key={user.id}
                  user={user}
                  isAdding={addingUserId === user.id}
                  onAdd={handleAddMember}
                />
              ))}
            </div>
          </div>
        )}
      </div>

      {/* Member list */}
      <div className="rounded-xl border border-slate-200 bg-white shadow-sm">
        <div className="border-b border-slate-200 px-6 py-5">
          <div className="flex items-center justify-between gap-4">
            <div>
              <h2 className="text-base font-semibold text-slate-900">
                Project members
              </h2>
              <p className="mt-1 text-sm text-slate-500">
                People who have access to this project.
              </p>
            </div>
            <span className="rounded-full bg-slate-100 px-2.5 py-1 text-xs font-medium text-slate-600">
              {members.length} {members.length === 1 ? "member" : "members"}
            </span>
          </div>
        </div>

        {membersError && (
          <div className="mx-6 mt-5 rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
            {membersError}
          </div>
        )}

        {isLoadingMembers ? (
          <div className="px-6 py-10 text-center text-sm text-slate-500">
            Loading members...
          </div>
        ) : members.length === 0 ? (
          <div className="px-6 py-12 text-center">
            <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-indigo-50">
              <EmptyMembersIcon />
            </div>
            <h3 className="mt-4 text-sm font-semibold text-slate-900">
              No members yet
            </h3>
            <p className="mt-1 text-sm text-slate-500">
              Add your first member using the form above.
            </p>
          </div>
        ) : (
          <div className="divide-y divide-slate-200">
            {members.map((member) => (
              <MemberRow
                key={member.id}
                member={member}
                onRemove={handleRemoveMember}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
