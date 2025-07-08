import axios from '@/error-handling/axiosSetup';
import { ChatDto } from '@/types/ChatCompletions';
import { IModelOptions } from '@/types/Assistant';

function getApiUrl(endpoint: string): string {
  const rootApiUrl = import.meta.env.VITE_AGILECHAT_API_URL as string;
  return `${rootApiUrl}/api/ChatCompletions${endpoint}`;
}

// Add like to a message
export async function chat(chatDto: ChatDto) {
  const apiUrl = getApiUrl('');
  return axios.post<ReadableStream<Uint8Array>>(apiUrl, chatDto, {
    headers: {
      Accept: 'text/plain',
      'Content-Type': 'application/json',
    },
    responseType: 'stream',
    adapter: 'fetch',
  });
}

export async function getTextModels() {
  const apiUrl = getApiUrl('/config/textmodels');
  try {
    const res = await axios.get<IModelOptions>(apiUrl);
    return res.data;
  } catch {
    return null;
  }
}
