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
  filePreviewType: FilePreviewType;
  bingDeployed: boolean;
}

export enum FilePreviewType {
  None = 'None',
  Preview = 'Preview',
  Download = 'Download'
}
export const useSettingsStore = create<SettingsState>(() => ({
  settings: undefined
}));
