export interface ChatThread {
    id: string;
    name: string;
    useName: string;
    userId: string;
    createdAt: Date;
    lastMessageAt: Date;
    bookmarked: boolean;
    isDeleted: boolean;
    type: string;
    assistantMessage: string;
    assistantTitle: string;
    assistantId: string;
    extension: string[];
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