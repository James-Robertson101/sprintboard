import type { Issue, IssueStatus } from "./Issue";

// Derived from Issue so we don't depend on how the Priority type is exported.
export type Priority = Issue["priority"];

// Adjust to match your backend SprintStatus enum values.
export type SprintStatus = "Planned" | "Active" | "Completed";

export interface StatusCount {
  status: IssueStatus;
  count: number;
}

export interface PriorityCount {
  priority: Priority;
  count: number;
}

export interface AssigneeWorkload {
  userId: number | null;
  name: string; // "Unassigned" when userId is null
  avatarUrl: string | null;
  total: number;
  done: number;
}

export interface SprintSummary {
  sprintId: number;
  name: string;
  startDate: string;
  endDate: string;
  status: SprintStatus;
  totalIssues: number;
  completedIssues: number;
  completionPercent: number;
  byStatus: StatusCount[];
  byPriority: PriorityCount[];
  byAssignee: AssigneeWorkload[];
}

export interface BurndownPoint {
  date: string;
  remaining: number;
  ideal: number;
}

export interface VelocityPoint {
  sprintId: number;
  sprintName: string;
  committed: number;
  completed: number;
}

// Mirrors SprintResponseDto. If you already have a Sprint type, use that instead.
export interface SprintListItem {
  id: number;
  projectId: number;
  name: string;
  goal: string | null;
  startDate: string;
  endDate: string;
  status: SprintStatus;
  issueCount: number;
}
