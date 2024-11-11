import axios from '@/error-handling/axiosSetup';
import { getApiUri } from '@/services/uri-helpers';
import { FileMetadata } from '@/models/filemetadata';

// Function to fetch all files
export const getFiles = async (): Promise<FileMetadata[]> => {
  try {
    const url = getApiUri('files');
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
    const url = getApiUri('files');

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
