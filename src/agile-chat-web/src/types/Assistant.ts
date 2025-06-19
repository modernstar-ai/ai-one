import { PermissionsAccessControlModel } from '@/components/ui-extended/permissions-access-control';

export type Assistant = {
  id: string;
  name: string;
  description: string;
  greeting: string;
  type: AssistantType;
  ragType: RagType;
  status: AssistantStatus;
  promptOptions: AssistantPromptOptions;
  filterOptions: AssistantFilterOptions;
  accessControl: PermissionsAccessControlModel;
  createdDate: Date;
  lastModified: Date;
};

export enum AssistantType {
  Chat = 'Chat',
  Search = 'Search'
}

export enum RagType {
  Plugin = 'Plugin',
  AzureSearchChatDataSource = 'AzureSearchChatDataSource'
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
  indexName: string;
  limitKnowledgeToIndex: boolean;
  allowInThreadFileUploads: boolean;
  documentLimit: number;
  strictness?: number;
  folders: string[];
  tags: string[];
}
