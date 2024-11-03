
export enum AssistantStatus {
  Draft = 0,
  Published = 1,
  Archived = 2,
  Deleted = 3,
}
export enum AssistantType {
  Chat = 0,
  Search = 1,
}

export type Assistant = {
  id: string;
  name: string;
  description: string;
  type: AssistantType;
  greeting: string;
  systemMessage: string;
  group?: string;
  folder?: string;
  temperature: number;
  documentLimit: number
  status: AssistantStatus;
  createdAt: string;
  createdBy: string;
  updatedAt: string;
  updatedBy: string;
};

