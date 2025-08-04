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
  agentConfiguration: SelectAgentConfiguration;
};

export type InsertAssistant = Partial<Assistant>;

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

import * as z from 'zod';

export const SelectConnectedAgentSchema = z.object({
  agentId: z.string().min(1, 'Agent ID is required'),
  agentName: z.string().min(1, 'Agent name is required').regex(/^[a-zA-Z_]+$/, 'Agent name must contain only letters and underscores'),
  activationDescription: z.string().min(1, 'Activation description is required')
});

export type SelectConnectedAgent = z.infer<typeof SelectConnectedAgentSchema>;

export const SelectAgentConfigurationSchema = z.object({
  agentDescription: z.string().optional(),
  agentId: z.string(),
  agentName: z.string(),
  connectedAgents: z.array(SelectConnectedAgentSchema)
});

export interface SelectAgentConfiguration {
  agentDescription?: string;
  agentId: string;
  agentName: string;
  connectedAgents: SelectConnectedAgent[];
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
