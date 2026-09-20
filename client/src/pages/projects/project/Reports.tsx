import { useEffect, useMemo, useState } from "react";
import { useParams } from "react-router-dom";
import type { IssueStatus } from "../../../types/Issue";
import type {
  AssigneeWorkload,
  BurndownPoint,
  SprintListItem,
  SprintSummary,
  VelocityPoint,
} from "../../../types/Report";
import {
  getBurndown,
  getProjectSprints,
  getSprintSummary,
  getVelocity,
} from "../../../services/reportService";
import BurndownChart from "../../../components/reports/BurndownChart";
import VelocityChart from "../../../components/reports/VelocityChart";
import BreakdownBars, {
  type BreakdownItem,
} from "../../../components/reports/BreakdownBars";
import {
  daysLeft,
  formatDateRange,
} from "../../../components/reports/dateUtils";

// The API omits statuses with zero issues, so we fill them in from this list.
const STATUS_ORDER: IssueStatus[] = ["Todo", "InProgress", "InReview", "Done"];

const STATUS_LABELS: Record<string, string> = {
  Todo: "To do",
  InProgress: "In progress",
  InReview: "In review",
  Done: "Done",
};

const STATUS_BAR: Record<string, string> = {
  Todo: "bg-slate-400",
  InProgress: "bg-blue-500",
  InReview: "bg-amber-500",
  Done: "bg-emerald-500",
};

const PRIORITY_ORDER = [
  "Critical",
  "Highest",
  "High",
  "Medium",
  "Low",
  "Lowest",
];

const PRIORITY_BAR: Record<string, string> = {
  Critical: "bg-rose-600",
  Highest: "bg-rose-600",
  High: "bg-rose-500",
  Medium: "bg-amber-500",
  Low: "bg-sky-500",
  Lowest: "bg-slate-400",
};

function priorityRank(priority: string) {
  const index = PRIORITY_ORDER.indexOf(priority);
  return index === -1 ? 99 : index;
}

const SPRINT_BADGE: Record<string, string> = {
  Active: "bg-emerald-50 text-emerald-700 ring-emerald-200",
  Completed: "bg-slate-100 text-slate-600 ring-slate-200",
  Planned: "bg-amber-50 text-amber-700 ring-amber-200",
};

function Card({
  title,
  subtitle,
  children,
}: {
  title: string;
  subtitle?: string;
  children: React.ReactNode;
}) {
  return (
    <section className="rounded-lg border border-slate-200 bg-white p-5 shadow-sm">
      <div className="mb-4">
        <h2 className="text-base font-semibold text-slate-900">{title}</h2>
        {subtitle && (
          <p className="mt-0.5 text-sm text-slate-500">{subtitle}</p>
        )}
      </div>
      {children}
    </section>
  );
}

function StatCard({
  label,
  value,
  children,
}: {
  label: string;
  value: string | number;
  children?: React.ReactNode;
}) {
  return (
    <div className="rounded-lg border border-slate-200 bg-white p-5 shadow-sm">
      <p className="text-sm text-slate-500">{label}</p>
      <p className="mt-1 text-3xl font-bold tracking-tight text-slate-900">
        {value}
      </p>
      {children}
    </div>
  );
}

function AssigneeAvatar({ workload }: { workload: AssigneeWorkload }) {
  if (workload.avatarUrl) {
    return (
      <img
        src={workload.avatarUrl}
        alt=""
        className="h-8 w-8 rounded-full bg-slate-100 object-cover"
      />
    );
  }
  return (
    <div className="flex h-8 w-8 items-center justify-center rounded-full bg-slate-200 text-xs font-medium text-slate-500">
      {workload.userId === null ? "?" : workload.name.charAt(0).toUpperCase()}
    </div>
  );
}

function Reports() {
  const { projectId } = useParams();

  const [sprints, setSprints] = useState<SprintListItem[]>([]);
  const [velocity, setVelocity] = useState<VelocityPoint[]>([]);
  const [selectedSprintId, setSelectedSprintId] = useState<number | null>(null);
  const [summary, setSummary] = useState<SprintSummary | null>(null);
  const [burndown, setBurndown] = useState<BurndownPoint[]>([]);

  const [isLoadingProject, setIsLoadingProject] = useState(true);
  const [isLoadingSprint, setIsLoadingSprint] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // 1) Project-level data: sprint list + velocity. Picks a default sprint.
  useEffect(() => {
    if (!projectId) return;

    let isCancelled = false;

    async function loadProjectData() {
      setIsLoadingProject(true);
      setError(null);
      try {
        const [sprintsData, velocityData] = await Promise.all([
          getProjectSprints(projectId!),
          getVelocity(projectId!),
        ]);
        if (isCancelled) return;

        setSprints(sprintsData);
        setVelocity(velocityData);

        // Prefer the active sprint, otherwise the most recent one
        // (the API returns sprints newest-first).
        const initial =
          sprintsData.find((s) => s.status === "Active") ??
          sprintsData[0] ??
          null;
        setSelectedSprintId(initial?.id ?? null);
      } catch (err) {
        if (!isCancelled) {
          setError(
            err instanceof Error ? err.message : "Failed to load reports.",
          );
        }
      } finally {
        if (!isCancelled) setIsLoadingProject(false);
      }
    }

    loadProjectData();
    return () => {
      isCancelled = true;
    };
  }, [projectId]);

  // 2) Sprint-level data: summary + burndown for the selected sprint.
  useEffect(() => {
    if (!projectId || selectedSprintId === null) return;

    const sprintId = selectedSprintId;
    let isCancelled = false;

    async function loadSprintReport() {
      setIsLoadingSprint(true);
      setError(null);
      try {
        const [summaryData, burndownData] = await Promise.all([
          getSprintSummary(projectId!, sprintId),
          getBurndown(projectId!, sprintId),
        ]);
        if (!isCancelled) {
          setSummary(summaryData);
          setBurndown(burndownData);
        }
      } catch (err) {
        if (!isCancelled) {
          setError(
            err instanceof Error
              ? err.message
              : "Failed to load sprint report.",
          );
        }
      } finally {
        if (!isCancelled) setIsLoadingSprint(false);
      }
    }

    loadSprintReport();
    return () => {
      isCancelled = true;
    };
  }, [projectId, selectedSprintId]);

  const statusItems = useMemo<BreakdownItem[]>(() => {
    if (!summary) return [];
    return STATUS_ORDER.map((status) => ({
      label: STATUS_LABELS[status] ?? status,
      count: summary.byStatus.find((s) => s.status === status)?.count ?? 0,
      barClass: STATUS_BAR[status] ?? "bg-slate-400",
    }));
  }, [summary]);

  const priorityItems = useMemo<BreakdownItem[]>(() => {
    if (!summary) return [];
    return [...summary.byPriority]
      .sort((a, b) => priorityRank(a.priority) - priorityRank(b.priority))
      .map((p) => ({
        label: p.priority,
        count: p.count,
        barClass: PRIORITY_BAR[p.priority] ?? "bg-slate-400",
      }));
  }, [summary]);

  const workload = useMemo(() => {
    if (!summary) return [];
    return [...summary.byAssignee].sort((a, b) => b.total - a.total);
  }, [summary]);

  const averageVelocity = useMemo(() => {
    if (velocity.length === 0) return null;
    const sum = velocity.reduce((acc, v) => acc + v.completed, 0);
    return Math.round((sum / velocity.length) * 10) / 10;
  }, [velocity]);

  return (
    <main className="min-w-0 flex-1">
      <div className="px-6 py-8 lg:px-10">
        <div className="mb-6 flex flex-wrap items-end justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold tracking-tight text-slate-900">
              Reports
            </h1>
            <p className="mt-1 text-sm text-slate-500">
              Track sprint progress and team velocity.
            </p>
          </div>

          {sprints.length > 0 && (
            <div>
              <label
                htmlFor="sprint-select"
                className="mb-1 block text-xs font-medium text-slate-500"
              >
                Sprint
              </label>
              <select
                id="sprint-select"
                value={selectedSprintId ?? ""}
                onChange={(e) => setSelectedSprintId(Number(e.target.value))}
                className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900 shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-200"
              >
                {sprints.map((s) => (
                  <option key={s.id} value={s.id}>
                    {s.name} ({s.status})
                  </option>
                ))}
              </select>
            </div>
          )}
        </div>

        {error && (
          <div className="mb-6 rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
            {error}
          </div>
        )}

        {isLoadingProject ? (
          <p className="text-sm text-slate-500">Loading reports...</p>
        ) : sprints.length === 0 ? (
          <div className="rounded-lg border border-dashed border-slate-300 bg-white px-6 py-12 text-center">
            <p className="text-sm font-medium text-slate-900">No sprints yet</p>
            <p className="mt-1 text-sm text-slate-500">
              Create and start a sprint to see reports here.
            </p>
          </div>
        ) : (
          <div className="space-y-6">
            {isLoadingSprint || !summary ? (
              <p className="text-sm text-slate-500">Loading sprint report...</p>
            ) : (
              <>
                {/* Sprint header */}
                <div className="flex flex-wrap items-center gap-3">
                  <h2 className="text-lg font-semibold text-slate-900">
                    {summary.name}
                  </h2>
                  <span
                    className={`rounded-full px-2.5 py-0.5 text-xs font-medium ring-1 ring-inset ${
                      SPRINT_BADGE[summary.status] ??
                      "bg-slate-100 text-slate-600 ring-slate-200"
                    }`}
                  >
                    {summary.status}
                  </span>
                  <span className="text-sm text-slate-500">
                    {formatDateRange(summary.startDate, summary.endDate)}
                  </span>
                </div>

                {/* Stat cards */}
                <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
                  <StatCard label="Total issues" value={summary.totalIssues} />
                  <StatCard label="Completed" value={summary.completedIssues} />
                  <StatCard
                    label="Progress"
                    value={`${summary.completionPercent}%`}
                  >
                    <div className="mt-3 h-2 overflow-hidden rounded-full bg-slate-100">
                      <div
                        className="h-full rounded-full bg-emerald-500"
                        style={{ width: `${summary.completionPercent}%` }}
                      />
                    </div>
                  </StatCard>
                  {summary.status === "Active" ? (
                    <StatCard
                      label="Days left"
                      value={daysLeft(summary.endDate)}
                    />
                  ) : (
                    <StatCard
                      label="Remaining"
                      value={summary.totalIssues - summary.completedIssues}
                    />
                  )}
                </div>

                {/* Burndown */}
                <Card
                  title="Burndown"
                  subtitle="Issues remaining over the course of the sprint."
                >
                  <BurndownChart
                    points={burndown}
                    startDate={summary.startDate}
                    endDate={summary.endDate}
                    totalIssues={summary.totalIssues}
                  />
                </Card>

                {/* Breakdowns */}
                <div className="grid gap-6 lg:grid-cols-2">
                  <Card title="By status">
                    <BreakdownBars
                      items={statusItems}
                      total={summary.totalIssues}
                    />
                  </Card>
                  <Card title="By priority">
                    <BreakdownBars
                      items={priorityItems}
                      total={summary.totalIssues}
                    />
                  </Card>
                </div>

                {/* Workload */}
                <Card title="Workload" subtitle="Issues per team member.">
                  {workload.length === 0 ? (
                    <p className="text-sm text-slate-500">
                      No issues in this sprint.
                    </p>
                  ) : (
                    <div className="overflow-x-auto">
                      <table className="w-full text-left text-sm">
                        <thead>
                          <tr className="border-b border-slate-200 text-xs uppercase tracking-wide text-slate-500">
                            <th className="pb-2 pr-4 font-medium">Member</th>
                            <th className="pb-2 pr-4 text-right font-medium">
                              Total
                            </th>
                            <th className="pb-2 pr-4 text-right font-medium">
                              Done
                            </th>
                            <th className="w-40 pb-2 font-medium">Progress</th>
                          </tr>
                        </thead>
                        <tbody className="divide-y divide-slate-100">
                          {workload.map((w) => {
                            const pct =
                              w.total === 0 ? 0 : (w.done / w.total) * 100;
                            return (
                              <tr key={w.userId ?? "unassigned"}>
                                <td className="py-3 pr-4">
                                  <div className="flex items-center gap-3">
                                    <AssigneeAvatar workload={w} />
                                    <span className="text-slate-900">
                                      {w.name}
                                    </span>
                                  </div>
                                </td>
                                <td className="py-3 pr-4 text-right tabular-nums text-slate-700">
                                  {w.total}
                                </td>
                                <td className="py-3 pr-4 text-right tabular-nums text-slate-700">
                                  {w.done}
                                </td>
                                <td className="py-3">
                                  <div className="h-2 overflow-hidden rounded-full bg-slate-100">
                                    <div
                                      className="h-full rounded-full bg-emerald-500"
                                      style={{ width: `${pct}%` }}
                                    />
                                  </div>
                                </td>
                              </tr>
                            );
                          })}
                        </tbody>
                      </table>
                    </div>
                  )}
                </Card>
              </>
            )}

            {/* Velocity is project-wide, so it shows regardless of selected sprint */}
            <Card
              title="Velocity"
              subtitle={
                averageVelocity === null
                  ? "Committed vs completed issues in recent sprints."
                  : `Committed vs completed issues in recent sprints. Average completed: ${averageVelocity} per sprint.`
              }
            >
              <VelocityChart data={velocity} />
            </Card>
          </div>
        )}
      </div>
    </main>
  );
}

export default Reports;
