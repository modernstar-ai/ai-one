import axios from '@/error-handling/axiosSetup';
import { IModelOptions } from '@/types/Assistant';

function getApiUrl(endpoint: string): string {
  const rootApiUrl = import.meta.env.VITE_AGILECHAT_API_URL as string;
  return `${rootApiUrl}/api/ChatConfig/${endpoint}`;
}

export async function getTextModels() {
  const apiUrl = getApiUrl('textmodels');
  try {
    const res = await axios.get<IModelOptions>(apiUrl);
    return res.data;
  } catch {
    return null;
  }
}
