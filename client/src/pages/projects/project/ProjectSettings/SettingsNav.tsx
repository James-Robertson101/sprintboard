import type { ReactNode } from "react";
import { GeneralIcon, MembersIcon, DangerIcon } from "./icons";

type SettingsSection = "general" | "members" | "danger";

interface SettingsNavProps {
  activeSection: SettingsSection;
  onSectionChange: (section: SettingsSection) => void;
}

const NAV_ITEMS: {
  key: SettingsSection;
  label: string;
  icon: ReactNode;
  danger?: boolean;
}[] = [
  { key: "general", label: "General", icon: <GeneralIcon /> },
  { key: "members", label: "Members", icon: <MembersIcon /> },
  { key: "danger", label: "Danger zone", icon: <DangerIcon />, danger: true },
];

export function SettingsNav({
  activeSection,
  onSectionChange,
}: SettingsNavProps) {
  return (
    <nav className="rounded-xl border border-slate-200 bg-white p-2 shadow-sm">
      {NAV_ITEMS.map((item) => (
        <div key={item.key}>
          {item.danger && <div className="my-2 border-t border-slate-100" />}
          <button
            type="button"
            onClick={() => onSectionChange(item.key)}
            className={`flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-left text-sm font-medium transition ${
              activeSection === item.key
                ? item.danger
                  ? "bg-rose-50 text-rose-700"
                  : "bg-indigo-50 text-indigo-700"
                : "text-slate-600 hover:bg-slate-50 hover:text-slate-900"
            }`}
          >
            {item.icon}
            {item.label}
          </button>
        </div>
      ))}
    </nav>
  );
}

export type { SettingsSection };
