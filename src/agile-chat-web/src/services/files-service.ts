import axios from '@/error-handling/axiosSetup';
import { CosmosFile } from '@/models/filemetadata';
import { PagedResultsDto } from '@/models/pagedResultsDto';
import { QueryDto } from '@/models/querydto';
import { AxiosResponse } from 'axios';

function getApiUrl(endpoint: string = ''): string {
  const rootApiUrl = import.meta.env.VITE_AGILECHAT_API_URL as string;
  return `${rootApiUrl}/api/files/${endpoint}`;
}

export async function getFolders(): Promise<string[]> {
  const apiUrl = getApiUrl('folders');

  try {
    const response = await axios.get<string[]>(apiUrl, { responseType: 'stream' });
    return response.data;
  } catch (error) {
    console.error('Error fetching folders:', error);
    return [];
  }
}

export async function uploadFiles(formData: FormData): Promise<CosmosFile | null> {
  const apiUrl = getApiUrl('');

  try {
    const response = await axios.post<CosmosFile>(apiUrl, formData);
    return response.data;
  } catch (error) {
    console.error('Error uploading files:', error);
    return null;
  }
}

// Function to fetch all files
export const getFiles = async (queryDto: QueryDto): Promise<PagedResultsDto<CosmosFile>> => {
  try {
    const params = new URLSearchParams();
    params.append('Page', queryDto.page.toString());
    params.append('PageSize', queryDto.pageSize.toString());
    queryDto.search && params.append('Search', queryDto.search);
    queryDto.orderBy && params.append('OrderBy', queryDto.orderBy);
    queryDto.orderType && params.append('OrderType', queryDto.orderType);
    console.log(queryDto);
    const url = getApiUrl('');
    const response = await axios.get<PagedResultsDto<CosmosFile>>(url, { params: params });
    return response.data;
  } catch (error) {
    console.error('Error fetching files from API:', error);
    throw error; // Re-throw the error to let the calling function know there was an issue
  }
};

// Function to delete selected files
export const deleteFiles = async (fileId: string): Promise<void> => {
  try {
    const url = getApiUrl(fileId);

    await axios.request({
      method: 'DELETE',
      url: url
    });
  } catch (error) {
    console.error('Error deleting files from API:', error);
    throw error; // Re-throw the error to let the calling function know there was an issue
  }
};

export const downloadFile = async (blobUrl: string): Promise<AxiosResponse> => {
  try {
    const url = getApiUrl('download');

    return await axios.post(url, { url: blobUrl }, { responseType: 'stream' });
  } catch (error) {
    console.error('Error deleting files from API:', error);
    throw error; // Re-throw the error to let the calling function know there was an issue
  }
};

export const GenerateSharedLinkByUrl = async (blobUrl: string): Promise<string> => {
  try {
    const url = getApiUrl('share');
    const resp = await axios.post<string>(url, { url: blobUrl });
    return resp.data;
  } catch (error) {
    console.error('Error deleting files from API:', error);
    throw error; // Re-throw the error to let the calling function know there was an issue
  }
};
