export interface Project {
  id: number;
  name: string;
  description: string | null;
  icon: string | null;
}

export type ProjectData = {
  name: string;
  description?: string;
  avatarUrl?: string;
};

export type ProjectMemberRole = "Owner" | "Member";
export interface ProjectMember {
  id: number;
  name: string;
  email: string;
  avatarUrl?: string | null;
  role: ProjectMemberRole;
}

export interface UserSummary {
  id: number;
  name: string;
  avatarUrl?: string;
}
