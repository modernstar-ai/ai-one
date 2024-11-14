// src/services/personaservice.ts
import axios from '@/error-handling/axiosSetup';
import { Indexes } from '@/types/Indexes';

function getApiUrl(endpoint: string): string {
  const rootApiUrl = import.meta.env.VITE_AGILECHAT_API_URL as string;
  return `${rootApiUrl}/api/indexes/${endpoint}`;
}

export async function getIndexes(): Promise<string[]> {
  const apiUrl = getApiUrl('');

  try {
    const response = await axios.get<string[]>(apiUrl);
    return response.data;
  } catch (error) {
    console.error('Error fetching indexes:', error);
    return [];
  }
}

//Create new index
export async function createIndex(newIndex: Indexes): Promise<Indexes | null> {
  const apiUrl = getApiUrl('assistants');
  try {
    // Ensure all required fields are included in the request
    const indexData = {
      name: newIndex.name,
      description: newIndex.description,
      group:newIndex.group,
      createdAt: newIndex.createdAt,
      createdBy: newIndex.createdBy,
    };

    const response = await axios.post<Indexes>(apiUrl, indexData);
    return response.data;
  } catch (error) {
    console.error('Error creating assistant:', error);
    return null;
  }
}