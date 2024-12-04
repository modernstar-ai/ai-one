import axios from '@/error-handling/axiosSetup';

function getApiUrl(endpoint: string): string {
  const rootApiUrl = import.meta.env.VITE_AGILECHAT_API_URL as string;
  return `${rootApiUrl}/api/AiSearch/${endpoint}`;
}

export async function getCitationChunkById(indexName: string, chunkId: string): Promise<string> {
  const apiUrl = getApiUrl(`${indexName}/${chunkId}`);
  try {
    const response = await axios.get<string>(apiUrl);
    return response.data;
  } catch (error) {
    console.error('Error fetching chunk:', error);
    return '';
  }
}
