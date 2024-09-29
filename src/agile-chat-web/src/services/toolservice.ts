import axios from 'axios';
import { Tool } from '../types/Tool';

function getApiUrl(endpoint: string): string {
  const rootApiUrl = import.meta.env.VITE_AGILECHAT_API_URL as string;
  return `${rootApiUrl}/${endpoint}`;
}

export async function fetchTools(): Promise<Tool[] | null> {  
 
  const apiUrl = getApiUrl('tools');
   
  try {
    const response = await axios.get<Tool[]>(apiUrl); // axios returns a response typed as Tool[]
    return response.data; // The data is directly available in response.data
  } catch (error) {
    console.error('Error fetching tools:', error);
    return null;
  }
}
