import axios from '@/error-handling/axiosSetup';
import { Assistant, InsertAssistant } from '../types/Assistant';

function getApiUrl(endpoint: string): string {
  const rootApiUrl = import.meta.env.VITE_AGILECHAT_API_URL as string;
  return `${rootApiUrl}/api/assistants/${endpoint}`;
}

// Fetch all assistants
export async function fetchAssistants(): Promise<Assistant[] | null> {
  const apiUrl = getApiUrl('');
  try {
    const response = await axios.get<Assistant[]>(apiUrl);
    return response.data;
  } catch (error) {
    console.error('Error fetching assistants:', error);
    return null;
  }
}

// Fetch a assistant by ID
export async function fetchAssistantById(id: string): Promise<Assistant | null> {
  try {
    if (id) {
      const apiUrl = getApiUrl(`${id}`);
      const response = await axios.get<Assistant>(apiUrl);
      return response.data;
    } else {
      console.error('No ID provided to fetch assistant');
      return null;
    }
  } catch (error) {
    console.error(`Error fetching assistant with ID ${id}:`, error);
    return null;
  }
}

// Create a new assistant
export async function createAssistant(newAssistant: InsertAssistant): Promise<Assistant> {
  const apiUrl = getApiUrl('');
  const response = await axios.post<Assistant>(apiUrl, newAssistant);
  return response.data;
}

export async function updateAssistant(updatedAssistant: InsertAssistant, id: string) {
  const apiUrl = getApiUrl(`${id}`);
  // Ensure all required fields are included in the request
  const response = await axios.put<Assistant>(apiUrl, updatedAssistant, {
    headers: {
      'Content-Type': 'application/json'
    }
  });

  if (response.status == 204) {
    return updatedAssistant;
  } else {
    return response.data;
  }
}

// Delete a assistant
export async function deleteAssistant(id: string): Promise<boolean> {
  const apiUrl = getApiUrl(`${id}`);
  try {
    await axios.delete(apiUrl);
    return true;
  } catch (error) {
    console.error(`Error deleting assistant with ID ${id}:`, error);
    return false;
  }
}
