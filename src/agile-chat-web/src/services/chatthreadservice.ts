import axios from '@/error-handling/axiosSetup';
import { ChatThread, CreateChatThread, Message, MessageOptions } from '@/types/ChatThread';

function getApiUrl(endpoint: string): string {
  const rootApiUrl = import.meta.env.VITE_AGILECHAT_API_URL as string;
  return `${rootApiUrl}/api/ChatThreads${endpoint}`;
}

// Add like to a message
export async function updateReaction(messageId: string, options: MessageOptions): Promise<boolean> {
  const apiUrl = getApiUrl(`/Messages/${messageId}`);
  try {
    await axios.put(apiUrl, options);
    return true;
  } catch {
    return false;
  }
}

// Fetch chat threads by user ID
export async function fetchChatThreads(): Promise<ChatThread[] | null> {
  const apiUrl = getApiUrl('');
  try {
    const response = await axios.get<ChatThread[]>(apiUrl);
    return response.data;
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
    return response.data;
  } catch {
    return null;
  }
}

export async function createChatThread(data: CreateChatThread): Promise<ChatThread | null> {
  const apiUrl = getApiUrl('');

  try {
    const response = await axios.post<ChatThread>(apiUrl, data);
    return { ...response.data };
  } catch {
    return null;
  }
}

export async function updateChatThread(chatThread: ChatThread): Promise<ChatThread | null> {
  const apiUrl = getApiUrl(`/${chatThread.id}`);
  try {
    const response = await axios.put<ChatThread>(apiUrl, chatThread);
    return response.data;
  } catch {
    return null;
  }
}

export async function deleteChatThread(id: string): Promise<boolean> {
  const apiUrl = getApiUrl(`/${id}`);
  try {
    await axios.delete(apiUrl, {
      headers: { 'Content-Type': 'application/json' }
    });
    return true;
  } catch {
    return false;
  }
}

export async function GetChatThreadMessages(chatThreadId: string): Promise<Message[]> {
  const apiUrl = getApiUrl(`/Messages/${chatThreadId}`);
  try {
    const messages = await axios.get<Message[]>(apiUrl);
    return messages.data;
  } catch {
    return [];
  }
}
