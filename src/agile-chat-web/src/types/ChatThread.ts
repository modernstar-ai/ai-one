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
}

export enum ChatType {
  Message = 'Message',
  Thread = 'Thread',
}

export enum MessageType {
  User = 'User',
  Assistant = 'Assistant',
}

export interface Message {
  id: string;
  content: string;
  type: MessageType;
  threadId: string;
  options: MessageOptions;
  createdDate: Date;
  lastModified: Date;
}

export interface MessageOptions {
  isLiked: boolean;
  isDisliked: boolean;
}
