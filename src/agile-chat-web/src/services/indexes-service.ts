// src/services/personaservice.ts
import axios from '@/error-handling/axiosSetup';
import { CreateIndexDto, Index, IndexReport } from '@/models/indexmetadata';

function getApiUrl(endpoint: string): string {
  const rootApiUrl = import.meta.env.VITE_AGILECHAT_API_URL as string;
  return `${rootApiUrl}/api/indexes/${endpoint}`;
}

export async function getIndexes(): Promise<Index[]> {
  const apiUrl = getApiUrl('');

  try {
    const response = await axios.get<Index[]>(apiUrl);
    return response.data;
  } catch (error) {
    console.error('Error fetching indexes:', error);
    return [];
  }
}

//Create new index in cosmos
export async function createIndex(newIndex: CreateIndexDto): Promise<Index | null> {
  const apiUrl = getApiUrl('');
  try {
    const response = await axios.post<Index>(apiUrl, newIndex, {
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

//Update index in cosmos
export async function updateIndex(body: { id: string; group: string; description: string }): Promise<void> {
  const apiUrl = getApiUrl(body.id);
  try {
    await axios.put(apiUrl, body, {
      headers: {
        'Content-Type': 'application/json',
      },
    });
  } catch (error) {
    console.error('Error creating index:', error);
  }
}

//Delete index in cosmos
export async function deleteIndex(id: string): Promise<boolean> {
  const apiUrl = getApiUrl(`${id}`);
  try {
    await axios.delete(apiUrl);
    return true;
  } catch (error) {
    console.error(`Error deleting index with ID ${id}:`, error);
    return false;
  }
}

export async function getIndexReport(reportName?: string): Promise<IndexReport | null> {
 
     const rootApiUrl = import.meta.env.VITE_AGILECHAT_API_URL as string;
     const apiUrl = `${rootApiUrl}/api/AiSearch/indexreport/${reportName}`;
  try {
    const response = await axios.get<IndexReport>(apiUrl);
    return response.data;
  } catch (error) {
    console.error('Error fetching index report:', error);
    return null;
  }
}
