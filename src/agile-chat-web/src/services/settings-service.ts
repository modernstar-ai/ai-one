import axios from '@/error-handling/axiosSetup';
import { AppSettings, useSettingsStore } from '@/stores/settings-store';

function getApiUrl(endpoint: string = ''): string {
  const rootApiUrl = import.meta.env.VITE_AGILECHAT_API_URL as string;
  return `${rootApiUrl}/api/settings/${endpoint}`;
}

export async function getSettings(): Promise<void> {
  const apiUrl = getApiUrl('');

  try {
    const response = await axios.get<AppSettings>(apiUrl);
    useSettingsStore.setState({
      settings: response.data
    });
  } catch (error) {
    console.error('Error fetching settings:', error);
  }
}
