export type Index = {
  id: string;
  name: string;
  description?: string;
  group?: string;
  createdAt: string;
  createdBy: string;
};

export type CreateIndexDto = {
  name: string;
  description?: string;
  group?: string;
};
