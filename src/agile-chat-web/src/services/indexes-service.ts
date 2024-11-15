// src/services/personaservice.ts
import axios from '@/error-handling/axiosSetup';
import { Indexes } from '@/models/indexmetadata';

function getApiUrl(endpoint: string): string {
  const rootApiUrl = import.meta.env.VITE_AGILECHAT_API_URL as string;
  return `${rootApiUrl}/api/indexes/${endpoint}`;
}

export async function getIndexes(): Promise<Indexes[]> {
  const apiUrl = getApiUrl('');

  try {
    const response = await axios.get<Indexes[]>(apiUrl);
    return response.data;
  } catch (error) {
    console.error('Error fetching indexes:', error);
    return [];
  }
}

//Create new index in cosmos
export async function createIndex(newIndex: Partial<Indexes>): Promise<Indexes | null> {
  const apiUrl = getApiUrl('create');
  try {
    const indexData = {
      name: newIndex.name,
      description: newIndex.description,
      group: newIndex.group,
      createdBy: newIndex.createdBy,
    };
    const response = await axios.post<Indexes>(apiUrl, indexData, {
      headers: {
        'Content-Type': 'application/json',
      },
    });

    return response.data;
  } catch (error) {
    console.error('Error creating index:', error);
    return null;
  }
}

//Delete index in cosmos
export async function deleteIndex(id: string): Promise<boolean> {
  const apiUrl = getApiUrl(`delete/${id}`);
  try {
    await axios.delete(apiUrl);
    return true;
  } catch (error) {
    console.error(`Error deleting index with ID ${id}:`, error);
    return false;
  }
}