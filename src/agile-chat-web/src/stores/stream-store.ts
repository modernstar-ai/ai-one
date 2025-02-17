import { create } from 'zustand';

interface StreamStore {
  stream: string | undefined;
  setStream: (newStream: string) => void;
  clearStream: () => void;
  callback: ((newStream: string) => void) | undefined;
}

const useStreamStore = create<StreamStore>((set) => ({
  stream: undefined,
  setStream: (newStream) => {
    set({ stream: newStream });
    const state = useStreamStore.getState();
    state.callback?.(newStream!);
  },
  clearStream: () => set({ stream: undefined, callback: undefined }),
  callback: undefined
}));

export default useStreamStore;
