export type Assistant = {
  id: string;
  name: string;
  description: string;
  greeting: string;
  type: AssistantType;
  status: AssistantStatus;
  promptOptions: AssistantPromptOptions;
  filterOptions: AssistantFilterOptions;
  createdDate: Date;
  lastModified: Date;
};

export enum AssistantType {
  Chat = 'Chat',
  Search = 'Search'
}

export enum AssistantStatus {
  Draft = 'Draft',
  Published = 'Published',
  Archived = 'Archived'
}

export interface AssistantPromptOptions {
  systemPrompt: string;
  temperature: number;
  topP: number;
  maxTokens: number;
}

export interface AssistantFilterOptions {
  group?: string;
  indexName: string;
  limitKnowledgeToIndex: boolean;
  documentLimit: number;
  strictness?: number;
  folders: string[];
}
