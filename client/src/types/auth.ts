export type RegisterData = {
  name: string;
  email: string;
  password: string;
  avatarUrl: string;
};

export type LoginData = {
  email: string;
  password: string;
};

export type User = {
  id: number;
  name: string;
  email: string;
  avatarUrl: string;
  role: UserRole;
};

export type UserRole = "Admin" | "User";
