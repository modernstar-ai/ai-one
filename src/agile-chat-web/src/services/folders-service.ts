// src/services/personaservice.ts
import axios from '@/error-handling/axiosSetup';
import { Folder } from '@/types/Folder';

function getApiUrl(endpoint: string): string {
  const rootApiUrl = import.meta.env.VITE_AGILECHAT_API_URL as string;
  return `${rootApiUrl}/api/file/${endpoint}`;
}

export async function getFolders(): Promise<Folder[]> {
  const apiUrl = getApiUrl('folders');

  try {
    const response = await axios.get<Folder[]>(apiUrl);
    return response.data;
  } catch (error) {
    console.error('Error fetching folders:', error);
    return [];
  }
}
