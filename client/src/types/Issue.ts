// Adjust these if your backend serializes enums as numbers instead of strings
// (i.e. if you're not using JsonStringEnumConverter in SprintBoard.Api).
export type Priority = "Low" | "Medium" | "High";
export type IssueStatus = "Todo" | "InProgress" | "InReview" | "Done";

export interface Assignee {
  id: number;
  name: string;
  avatarUrl?: string | null;
}

// Mirrors IssueResponseDto
export interface Issue {
  id: number;
  name: string;
  description?: string | null;
  priority: Priority;
  status: IssueStatus;
  assignee?: Assignee | null;
  createdAt: string;
  updatedAt?: string | null;
}

// Mirrors CreateIssueDto
export interface CreateIssuePayload {
  name: string;
  description?: string | null;
  priority: Priority;
  assigneeId?: number | null;
}

// Mirrors UpdateIssueDto
export interface UpdateIssuePayload {
  name: string;
  description?: string | null;
  priority: Priority;
  status: IssueStatus;
  assigneeId?: number | null;
}
