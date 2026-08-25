export type AvatarOption = {
  id: string;
  name: string;
  url: string;
};

export type AvatarCategory = {
  id: string;
  name: string;
  options: AvatarOption[];
};
