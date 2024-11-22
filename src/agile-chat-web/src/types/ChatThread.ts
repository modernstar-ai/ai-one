export interface ChatThread {
  id: string;
  name: string;
  userName: string;
  userId: string;
  type: string;
  createdAt: Date;
  lastMessageAt: Date;
  updatedAt: Date;
  bookmarked: boolean;
  isDeleted: boolean;
  assistantMessage: string;
  assistantTitle: string;
  assistantId: string;
  extension: string[];
  temperature: number | null;
  topP: number | null;
  maxResponseToken: number | null;
  strictness: number | null;
  documentLimit: number;
}
  
 
  export interface Message {
      id: string;
      createdAt: Date;
      type: string;
      isDeleted: boolean;
      content: string;
      name: string;
      role: string;
      threadId: string;
      userId: string;
      multiModalImage: string;
      sender: 'function' | 'user' | 'assistant' | 'system' | 'tool' ;  // Changed from string to SenderType
  }
  
  export interface NewChatThread {
    name: string;
    personaMessage?: string;
    personaMessageTitle?: string;
    userId: string;
    extension?: string[];
  }
  
  export interface UpdateChatThreadTitle {
    id: string;
    title: string;
  }
  
  export interface ExtensionUpdate {
    chatThreadId: string;
    extensionId: string;
  }