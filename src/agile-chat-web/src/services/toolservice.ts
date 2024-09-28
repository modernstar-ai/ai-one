// src/services/api.ts
import axios from 'axios';
import { Tool } from '../types/Tool';

export async function fetchTools(): Promise<Tool[] | null> {  
    const apiUrl = import.meta.env.VITE_AGILECHAT_API_URL as string;
    // console.log('API URL:', apiUrl);
  try {
    const response = await axios.get<Tool[]>(`${apiUrl}/tools`); // axios returns a response typed as Tool[]
    return response.data; // The data is directly available in response.data
  } catch (error) {
    console.error('Error fetching tools:', error);
    return null;
  }
}
