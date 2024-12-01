export type Assistant = {
  id: string;
  name: string;
  description: string;
  greeting: string;
  status: AssistantStatus;
  promptOptions: AssistantPromptOptions;
  filterOptions: AssistantFilterOptions;
  createdDate: Date;
  lastModified: Date;
};

export enum AssistantStatus {
  Draft = 'Draft',
  Published = 'Published',
  Archived = 'Archived',
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
  documentLimit: number;
  strictness?: number;
}
