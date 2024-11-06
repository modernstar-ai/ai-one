export enum AssistantStatus {
  Draft = 'Draft',
  Published = 'Published',
  Archived = 'Archived',
  Deleted = 'Deleted',
}
export enum AssistantType {
  Chat = 'Chat',
  Search = 'Search',
}
export class Tools {
  toolId: string = '';
  toolName: string = '';
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
  documentLimit: number;
  status: AssistantStatus;
  createdAt: string;
  createdBy: string;
  updatedAt: string;
  updatedBy: string;
  tools: Tools[];
};
