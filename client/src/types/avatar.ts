export type AvatarOption = {
  id: string;
  style: string;
  seed: string;
  url: string;
};

export type AvatarCategory = {
  id: string;
  name: string;
  options: AvatarOption[];
};
