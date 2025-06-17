export interface ChatThread {
  id: string;
  name: string;
  userId: string;
  type: ChatType;
  isBookmarked: boolean;
  assistantId?: string;
  promptOptions: ChatThreadPromptOptions;
  filterOptions: ChatThreadFilterOptions;
  createdDate: Date;
  lastModified: Date;
}

export interface CreateChatThread {
  name?: string;
  assistantId?: string;
  promptOptions?: ChatThreadPromptOptions;
  filterOptions?: ChatThreadFilterOptions;
}

export interface ChatThreadPromptOptions {
  systemPrompt: string | undefined;
  temperature: number | undefined;
  topP: number | undefined;
  maxTokens: number | undefined;
}

export interface ChatThreadFilterOptions {
  documentLimit: number;
  strictness: number;
  folders: string[];
  tags: string[];
}

export enum ChatType {
  Thread = 'Thread',
  Message = 'Message'
}

export enum MessageType {
  User = 'User',
  Assistant = 'Assistant'
}

export interface ChatThreadFile {
  id: string;
  content: string;
  contentType: string;
  name: string;
  type: ChatType;
  size: number;
  threadId: string;
  url: string;
  createdDate: Date;
  lastModified: Date;
}

export interface Message {
  id: string;
  content: string;
  type: ChatType;
  messageType: MessageType;
  threadId: string;
  options: MessageOptions;
  createdDate: Date;
  lastModified: Date;
}

export type MessageOptions = { [key in MetadataType]?: any };

export enum MetadataType {
  Citations = 'Citations',
  IsLiked = 'IsLiked',
  IsDisliked = 'IsDisliked',
  SearchProcess = 'SearchProcess'
}

export interface Citation {
  id: string;
  name: string;
  content: string;
  url: string;
}

export interface SearchProcess {
  assistantResponse: string;
  thoughtProcess: string;
  Citations: Citation[];
}
