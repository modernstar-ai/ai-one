// src/services/personaservice.ts
import axios from '@/error-handling/axiosSetup';

function getApiUrl(endpoint: string): string {
  const rootApiUrl = import.meta.env.VITE_AGILECHAT_API_URL as string;
  return `${rootApiUrl}/api/file/${endpoint}`;
}

export async function getFolders(): Promise<string[]> {
  const apiUrl = getApiUrl('folders');

  try {
    const response = await axios.get<string[]>(apiUrl);
    return response.data;
  } catch (error) {
    console.error('Error fetching folders:', error);
    return [];
  }
}
