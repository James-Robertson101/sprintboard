export type SprintStatus = "Planned" | "Active" | "Completed";

export interface Sprint {
  id: number;
  projectId: number;
  name: string;
  goal: string | null;
  startDate: string;
  endDate: string;
  status: SprintStatus;
  issueCount: number;
}

export interface CreateSprintPayload {
  name: string;
  goal: string | null;
  startDate: string;
  endDate: string;
}

export interface UpdateSprintPayload {
  name: string;
  goal: string | null;
  startDate: string;
  endDate: string;
}
