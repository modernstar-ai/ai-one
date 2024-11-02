// import axios from 'axios';
// import { Tool } from '../types/Tool';

// function getApiUrl(endpoint: string): string {
//   const rootApiUrl = import.meta.env.VITE_AGILECHAT_API_URL as string;
//   return `${rootApiUrl}/${endpoint}`;
// }

// export async function fetchTools(): Promise<Tool[] | null> {

//   const apiUrl = getApiUrl('tools');

//   try {
//     const response = await axios.get<Tool[]>(apiUrl); // axios returns a response typed as Tool[]
//     return response.data; // The data is directly available in response.data
//   } catch (error) {
//     console.error('Error fetching tools:', error);
//     return null;
//   }
// }

import axios from '@/error-handling/axiosSetup';
import { Tool } from '../types/Tool';

function getApiUrl(endpoint: string): string {
  const rootApiUrl = import.meta.env.VITE_AGILECHAT_API_URL as string;
  return `${rootApiUrl}/${endpoint}`;
}

// Fetch all tools
export async function fetchTools(): Promise<Tool[] | null> {
  const apiUrl = getApiUrl('tools');
  try {
    const response = await axios.get<Tool[]>(apiUrl);
    return response.data;
  } catch (error) {
    console.error('Error fetching tools:', error);
    return null;
  }
}

// Fetch a tool by ID
export async function fetchToolById(id: string): Promise<Tool | null> {
  const apiUrl = getApiUrl(`tools/${id}`);
  try {
    const response = await axios.get<Tool>(apiUrl);
    return response.data;
  } catch (error) {
    console.error(`Error fetching tool with ID ${id}:`, error);
    return null;
  }
}

// Create a new tool
export async function createTool(newTool: Tool): Promise<Tool | null> {
  const apiUrl = getApiUrl('tools');
  try {
    const response = await axios.post<Tool>(apiUrl, newTool);
    return response.data;
  } catch (error) {
    console.error('Error creating tool:', error);
    return null;
  }
}

export async function updateTool(updatedTool: Tool): Promise<Tool | null> {
  const apiUrl = getApiUrl(`tools/${updatedTool.id}`);
  try {
    // Ensure all required fields are included in the request
    const toolData = {
      id: updatedTool.id,
      name: updatedTool.name,
      type: updatedTool.type,
      status: updatedTool.status,
      description: updatedTool.description,
      jsonTemplate: updatedTool.jsonTemplate,
      databaseDSN: updatedTool.databaseDSN,
      databaseQuery: updatedTool.databaseQuery,
      createddate: updatedTool.createddate,
      method: updatedTool.method,
      api: updatedTool.api,
      lastupdateddate: new Date().toISOString(), // Always update this timestamp
    };

    const response = await axios.put<Tool>(apiUrl, toolData, {
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (response.status == 204) {
      return toolData;
    } else {
      return response.data;
    }
  } catch (error) {
    console.error(`Error updating tool with ID ${updatedTool.id}:`, error);

    return null;
  }
}

// Delete a tool
export async function deleteTool(id: string): Promise<boolean> {
  const apiUrl = getApiUrl(`tools/${id}`);
  try {
    await axios.delete(apiUrl);
    return true;
  } catch (error) {
    console.error(`Error deleting tool with ID ${id}:`, error);
    return false;
  }
}
