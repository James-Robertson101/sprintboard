import type { Assignee } from "./Issue";

export interface Comment {
  id: number;
  issueId: number;
  content: string;
  author: Assignee;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateCommentPayload {
  content: string;
}

export interface UpdateCommentPayload {
  content: string;
}
