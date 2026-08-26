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
