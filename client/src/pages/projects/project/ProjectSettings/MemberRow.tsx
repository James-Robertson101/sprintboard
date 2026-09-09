import type { ProjectMember } from "../../../../types/project";
import { RemoveIcon } from "./icons";

interface MemberRowProps {
  member: ProjectMember;
  onRemove: (memberId: number) => void;
}

export function MemberRow({ member, onRemove }: MemberRowProps) {
  return (
    <div className="flex items-center justify-between gap-4 px-6 py-4">
      <div className="flex min-w-0 items-center gap-3">
        {member.avatarUrl ? (
          <img
            src={member.avatarUrl}
            alt={member.name}
            className="h-10 w-10 shrink-0 rounded-full object-cover"
          />
        ) : (
          <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-indigo-100 text-sm font-semibold text-indigo-700">
            {member.name.charAt(0).toUpperCase()}
          </div>
        )}
        <div className="min-w-0">
          <p className="truncate text-sm font-medium text-slate-900">
            {member.name}
          </p>
          <p className="text-sm text-slate-500">{member.role}</p>
        </div>
      </div>

      <div className="flex shrink-0 items-center gap-3">
        <span
          className={`rounded-full px-2.5 py-1 text-xs font-medium ${
            member.role === "Owner"
              ? "bg-indigo-50 text-indigo-700"
              : "bg-slate-100 text-slate-600"
          }`}
        >
          {member.role}
        </span>

        {member.role !== "Owner" && (
          <button
            type="button"
            onClick={() => onRemove(member.id)}
            className="rounded-lg p-2 text-slate-400 transition hover:bg-rose-50 hover:text-rose-600"
            title="Remove member"
          >
            <RemoveIcon />
          </button>
        )}
      </div>
    </div>
  );
}
