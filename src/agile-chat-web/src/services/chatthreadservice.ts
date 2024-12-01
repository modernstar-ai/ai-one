import axios from '@/error-handling/axiosSetup';
import { ChatThread, CreateChatThread, Message, UpdateChatThreadTitle } from '@/types/ChatThread';
import { Assistant } from '@/types/Assistant';

function getApiUrl(endpoint: string): string {
  const rootApiUrl = import.meta.env.VITE_AGILECHAT_API_URL as string;
  return `${rootApiUrl}/api/ChatThreads${endpoint}`;
}

// Add like to a message
export async function addLikeReaction(messageId: string, userId: string): Promise<boolean> {
  const apiUrl = getApiUrl(`/messagereaction/like/${messageId}`);
  try {
    await axios.post(apiUrl, null, {
      params: { userId },
    });
    return true;
  } catch {
    return false;
  }
}

// Remove like from a message
export async function removeLikeReaction(messageId: string, userId: string): Promise<boolean> {
  const apiUrl = getApiUrl(`/messagereaction/removelike/${messageId}`);
  try {
    await axios.post(apiUrl, null, {
      params: { userId },
    });
    return true;
  } catch {
    return false;
  }
}

// Add dislike to a message
export async function addDislikeReaction(messageId: string, userId: string): Promise<boolean> {
  const apiUrl = getApiUrl(`/messagereaction/dislike/${messageId}`);
  try {
    await axios.post(apiUrl, null, {
      params: { userId },
    });
    return true;
  } catch {
    return false;
  }
}

// Remove dislike from a message
export async function removeDislikeReaction(messageId: string, userId: string): Promise<boolean> {
  const apiUrl = getApiUrl(`/messagereaction/removedislike/${messageId}`);
  try {
    await axios.post(apiUrl, null, {
      params: { userId },
    });
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
      // Handle strings
      id: response.data.id,
      name: response.data.name || 'Untitled Chat',
      userName: response.data.userName || 'Anonymous',
      userId: response.data.userId,
      type: response.data.type || 'CHAT_THREAD',
      assistantMessage: response.data.assistantMessage || '',
      assistantTitle: response.data.assistantTitle || 'Assistant',
      assistantId: response.data.assistantId || '',
      // Handle dates
      createdAt: new Date(response.data.createdAt),
      lastMessageAt: new Date(response.data.lastMessageAt),
      updatedAt: new Date(response.data.updatedAt),
      // Handle booleans
      bookmarked: response.data.bookmarked ?? false,
      isDeleted: response.data.isDeleted ?? false,
      // Handle arrays
      extension: response.data.extension || [],
      // Handle nullable numbers
      temperature: response.data.temperature ?? null,
      topP: response.data.topP ?? null,
      maxResponseToken: response.data.maxResponseToken ?? null,
      strictness: response.data.strictness ?? null,
      // Handle non-nullable number
      documentLimit: response.data.documentLimit ?? 0,
    };
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

export async function GetChatThreadMessages(chatThreadId: string): Promise<Message[]> {
  const apiUrl = getApiUrl(`/Messages/${chatThreadId}`);
  try {
    const messages = await axios.get<Message[]>(apiUrl);
    return messages.data;
  } catch {
    return [];
  }
}
