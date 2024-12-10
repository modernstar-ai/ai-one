import { create } from 'zustand';

interface StreamStore {
  stream: string | null;
  setStream: (newStream: string) => void;
  clearStream: () => void;
  callback: ((newStream: string) => void) | undefined;
}

const useStreamStore = create<StreamStore>((set) => ({
  stream: null,
  setStream: (newStream) => {
    set({ stream: newStream });
    const state = useStreamStore.getState();
    if (state.callback) {
      state.callback(newStream!);
    }
  },
  clearStream: () => set({ stream: null, callback: undefined }),
  callback: undefined,
}));

export default useStreamStore;
