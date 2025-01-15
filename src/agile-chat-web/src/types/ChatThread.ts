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
}

export enum ChatType {
  Thread = 'Thread',
  Message = 'Message',
}

export enum MessageType {
  User = 'User',
  Assistant = 'Assistant',
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

export interface MessageOptions {
  isLiked: boolean;
  isDisliked: boolean;
  metadata: { [key in MetadataType]?: object };
}

export enum MetadataType {
  Citations = 'Citations',
  SearchProcess = 'SearchProcess',
}

export interface Citation {
  id: string;
  name: string;
  url: string;
}

export interface SearchProcess {
  assistantResponse: string;
  thoughtProcess: string;
  Citations: Citation[];
}
