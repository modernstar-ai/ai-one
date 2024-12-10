import { create } from 'zustand';

export interface PermissionsState {
  roles: string[] | undefined;
  groups: string[] | undefined;
}

export const usePermissionsStore = create<PermissionsState>(() => ({
  groups: undefined,
  roles: undefined,
}));
