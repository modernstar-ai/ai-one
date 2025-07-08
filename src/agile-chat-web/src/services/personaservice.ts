// src/services/personaservice.ts
import axios from '@/error-handling/axiosSetup';
import { Persona } from '@/types/Persona';

function getApiUrl(endpoint: string): string {
  const rootApiUrl = import.meta.env.VITE_AGILECHAT_API_URL as string;
  return `${rootApiUrl}/${endpoint}`;
}

export async function fetchPersonas(): Promise<Persona[] | null> {
  const apiUrl = getApiUrl('personas');
  try {
    const response = await axios.get<Persona[]>(apiUrl);
    return response.data;
  } catch (error) {
    console.error('Error fetching personas:', error);
    return null;
  }
}

export async function addPersona(newPersona: Omit<Persona, 'id'>): Promise<Persona | null> {
  const apiUrl = getApiUrl('personas');
  try {
    const response = await axios.post<Persona>(apiUrl, newPersona);
    return response.data;
  } catch (error) {
    console.error('Error adding persona:', error);
    return null;
  }
}

export async function updatePersona(updatedPersona: Persona): Promise<Persona | null> {
  const apiUrl = getApiUrl(`personas/${updatedPersona.id}`);
  try {
    const response = await axios.put<Persona>(apiUrl, updatedPersona);
    return response.data;
  } catch (error) {
    console.error('Error updating persona:', error);
    return null;
  }
}

export async function deletePersona(id: string): Promise<boolean> {
  const apiUrl = getApiUrl(`personas/${id}`);
  try {
    await axios.delete(apiUrl);
    return true;
  } catch (error) {
    console.error('Error deleting persona:', error);
    return false;
  }
}
