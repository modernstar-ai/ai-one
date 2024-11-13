// src/services/personaservice.ts
import axios from '@/error-handling/axiosSetup';
import { FileMetadata } from '@/models/filemetadata';

function getApiUrl(endpoint: string = ''): string {
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

export async function uploadFiles(formData: FormData): Promise<string> {
  const apiUrl = getApiUrl('upload');

  try {
    const response = await axios.post<string>(apiUrl, formData);
    return response.data;
  } catch (error) {
    console.error('Error uploading files:', error);
    return 'Error uploading files: ' + error;
  }
}

// Function to fetch all files
export const getFiles = async (): Promise<FileMetadata[]> => {
  try {
    const url = getApiUrl('');
    const response = await axios.get<FileMetadata[]>(url);
    return response.data;
  } catch (error) {
    console.error('Error fetching files from API:', error);
    throw error; // Re-throw the error to let the calling function know there was an issue
  }
};

// Function to delete selected files
export const deleteFiles = async (fileIds: string[]): Promise<void> => {
  try {
    const url = getApiUrl('');

    await axios.request({
      method: 'DELETE',
      url: url,
      data: { fileIds: fileIds },
    });
  } catch (error) {
    console.error('Error deleting files from API:', error);
    throw error; // Re-throw the error to let the calling function know there was an issue
  }
};
