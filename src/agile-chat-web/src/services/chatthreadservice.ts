import axios from 'axios';
import { ChatThread, NewChatThread, Message, UpdateChatThreadTitle, ExtensionUpdate } from '@/types/ChatThread';
import { Assistant } from '@/types/Assistant';

function getApiUrl(endpoint: string): string {
  const rootApiUrl = import.meta.env.VITE_AGILECHAT_API_URL as string;
  return `${rootApiUrl}/chat-threads${endpoint}`;
}

// Fetch chat threads by user ID
export async function fetchChatThreads(userId: string): Promise<ChatThread[] | null> {
  const apiUrl = getApiUrl(`/user/${userId}`);
  try {
    const response = await axios.get<ChatThread[]>(apiUrl);
    return response.data.map((thread) => ({
      ...thread,
      createdAt: new Date(thread.createdAt),
      lastMessageAt: new Date(thread.lastMessageAt),
    }));
  } catch {
    return null;
  }
}

// Fetch chat messages by threadid
export async function fetchChatsbythreadid(threadId: string): Promise<Message[] | null> {
  const apiUrl = getApiUrl(`/threads/${threadId}`);
  try {
    const response = await axios.get<Message[]>(apiUrl);
    return response.data.map((thread) => ({ ...thread }));
  } catch {
    return null;
  }
}

export async function fetchChatThread(id: string): Promise<ChatThread | null> {
  const apiUrl = getApiUrl(`/${id}`);
  try {
    const response = await axios.get<ChatThread>(apiUrl);
    return {
      ...response.data,
      createdAt: new Date(response.data.createdAt),
      lastMessageAt: new Date(response.data.lastMessageAt),
    };
  } catch {
    return null;
  }
}

export async function createChatThread(data?: NewChatThread): Promise<ChatThread | null> {
  const apiUrl = getApiUrl('');

  // Get assistantId from query string if it exists
  const urlParams = new URLSearchParams(window.location.search);
  const assistantId = urlParams.get('assistantId');

  try {
    // Add assistantId to data if it exists in query string
    const threadData = assistantId ? { ...data, assistantId } : data;

    const response = await axios.post<ChatThread>(apiUrl, threadData);
    return { ...response.data };
  } catch {
    return null;
  }
}

export async function updateChatThread(chatThread: ChatThread): Promise<ChatThread | null> {
  const apiUrl = getApiUrl(`/${chatThread.id}`);
  try {
    const response = await axios.put<ChatThread>(apiUrl, chatThread);
    return {
      ...response.data,
      createdAt: new Date(response.data.createdAt),
      lastMessageAt: new Date(response.data.lastMessageAt),
    };
  } catch {
    return null;
  }
}

export async function updateChatTitle({ id, title }: UpdateChatThreadTitle): Promise<ChatThread | null> {
  const apiUrl = getApiUrl(`/${id}/title`);
  try {
    const response = await axios.patch<ChatThread>(apiUrl, {
      title: title.substring(0, 30),
    });
    return {
      ...response.data,
      createdAt: new Date(response.data.createdAt),
      lastMessageAt: new Date(response.data.lastMessageAt),
    };
  } catch {
    return null;
  }
}

export async function deleteChatThread(id: string, userid: string): Promise<boolean> {
  const apiUrl = getApiUrl(`/${id}`);
  try {
    await axios.delete(apiUrl, {
      params: { userid },
      headers: { 'Content-Type': 'application/json' },
    });
    return true;
  } catch {
    return false;
  }
}

export async function addExtensionToChatThread(data: ExtensionUpdate): Promise<ChatThread | null> {
  const apiUrl = getApiUrl(`/${data.chatThreadId}/extensions`);
  try {
    const response = await axios.post<ChatThread>(apiUrl, {
      extensionId: data.extensionId,
    });
    return {
      ...response.data,
      createdAt: new Date(response.data.createdAt),
      lastMessageAt: new Date(response.data.lastMessageAt),
    };
  } catch {
    return null;
  }
}

export async function removeExtensionFromChatThread(data: ExtensionUpdate): Promise<ChatThread | null> {
  const apiUrl = getApiUrl(`/${data.chatThreadId}/extensions/${data.extensionId}`);
  try {
    const response = await axios.delete<ChatThread>(apiUrl);
    return {
      ...response.data,
      createdAt: new Date(response.data.createdAt),
      lastMessageAt: new Date(response.data.lastMessageAt),
    };
  } catch {
    return null;
  }
}

export async function createChatAndRedirect(): Promise<void> {
  try {
    const newThread = await createChatThread({
      name: '',
      userId: '',
      personaMessage: '',
      personaMessageTitle: '',
    });
    if (newThread) {
      window.location.href = `/chat/${newThread.id}`;
    }
  } catch {
    return;
  }
}

export async function GetChatThreadMessages(
  username: string,
  chatThreadId: string,
  currentAssistant: Assistant | null
): Promise<Message[]> {
  console.log('Chat - chatThreadId:', chatThreadId, currentAssistant, username);
  let mergedMessages: Message[] = [];
  let initialMessages: Message[] = [];
  let existingMessages: Message[] | null;

  // if there is an assistant get the system and welcome messages
  //if (currentAssistant) {

    initialMessages = GetSystemAndWelcomeMessages(username, currentAssistant, chatThreadId);
    console.log('Chat - set initialMessages from current assistant:', initialMessages);
  //}

  if (chatThreadId) {
    // Load existing chat thread messages
    existingMessages = await fetchChatsbythreadid(chatThreadId);
    console.log('Chat - existingMessages:', existingMessages);

    if (existingMessages) {
      mergedMessages = [...initialMessages, ...existingMessages];
    } else {
      mergedMessages = initialMessages;
    }
  }
  return mergedMessages;
}

export function GetSystemAndWelcomeMessages(
  userName: string,
  currentAssistant: Assistant | null,
  chatThreadId: string
): Message[] {
  console.log('currentAssistant', currentAssistant);
  const systemMessage: Message = {
    id: crypto.randomUUID(),
    createdAt: new Date(),
    type: 'text',
    isDeleted: false,
    content: currentAssistant?.systemMessage || 'Hello! How can I assist you today?',
    name: 'System',
    role: 'system',
    threadId: chatThreadId,
    userId: userName,
    multiModalImage: '',
    sender: 'system',
  };

  //if (currentAssistant?.systemMessage && currentAssistant?.greeting) {
    const welcomeMessage: Message = {
      id: crypto.randomUUID(),
      createdAt: new Date(),
      type: 'text',
      isDeleted: false,
      content: currentAssistant?.greeting || 'Hello! How can I assist you today?',
      name: currentAssistant?.name || '',
      role: 'assistant',
      threadId: chatThreadId,
      userId: userName,
      multiModalImage: '',
      sender: 'assistant',
    };
    return [systemMessage, welcomeMessage];
    //} else {
    //return [systemMessage];
  //}
  
}

export type { ChatThread, NewChatThread, UpdateChatThreadTitle, ExtensionUpdate };
