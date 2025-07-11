import { create } from 'zustand';

export interface SettingsState {
  settings: AppSettings | undefined;
}

export interface AppSettings {
  appName?: string;
  aiDisclaimer?: string;
  logoUrl?: string;
  faviconUrl?: string;
  allowModelSelectionDefaultValue: string;
  defaultTextModelId: string;
  modelSelectionFeatureEnabled: boolean;
}

export const useSettingsStore = create<SettingsState>(() => ({
  settings: undefined
}));
