import { create } from 'zustand';

export interface SettingsState {
  settings: AppSettings | undefined;
}

export interface AppSettings {
  appName: string | undefined;
  aiDisclaimer: string | undefined;
  logoUrl: string | undefined;
  faviconUrl: string | undefined;
}

export const useSettingsStore = create<SettingsState>(() => ({
  settings: undefined
}));
