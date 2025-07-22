import { PermissionsAccessControlModel } from '@/components/ui-extended/permissions-access-control';

export type Assistant = {
  id: string;
  name: string;
  description: string;
  greeting: string;
  type: AssistantType;
  status: AssistantStatus;
  promptOptions: AssistantPromptOptions;
  filterOptions: AssistantFilterOptions;
  accessControl: PermissionsAccessControlModel;
  createdDate: Date;
  lastModified: Date;
  modelOptions: IModelOptions;
};

export type InsertAssistant = Partial<Assistant>;

// export interface ConnectedAgent {
//   agentType: string;
//   description: string;
// }

export enum AssistantType {
  Chat = 'Chat',
  Search = 'Search',
  Agent = 'Agent'
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

export interface IModelOptions {
  allowModelSelection: boolean;
  models: Model[];
  defaultModelId: string;
}

export interface Model {
  modelId: string;
  isSelected: boolean;
}

export interface AssistantFilterOptions {
  indexName: string;
  limitKnowledgeToIndex: boolean;
  allowInThreadFileUploads: boolean;
  documentLimit: number;
  strictness?: number;
  folders: string[];
  tags: string[];
}
