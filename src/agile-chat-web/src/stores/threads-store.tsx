import { fetchChatThreads } from '@/services/chatthreadservice';
import { ChatThread } from '@/types/ChatThread';
import { create } from 'zustand';

export interface ThreadsState {
  threads: ChatThread[] | undefined | null;
  refreshThreads: () => Promise<void>;
}

export const useThreadsStore = create<ThreadsState>((set) => ({
  threads: undefined,
  refreshThreads: async () => {
    const threads = await fetchChatThreads();
    set({ threads: threads });
  },
}));
