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
  index: string;
  folder?: string[];
  temperature: number;
  topP?: number;
  maxResponseToken?: number;
  pastMessages?: number;
  strictness?: number;
  documentLimit: number;
  tools: Tools[];
  status: AssistantStatus;
  createdAt: string;
  createdBy: string;
  updatedAt: string;
  updatedBy: string;
};
