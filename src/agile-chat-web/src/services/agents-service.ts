import axios from '@/error-handling/axiosSetup';
import { Assistant } from '@/types/Assistant';

const rootApiUrl = import.meta.env.VITE_AGILECHAT_API_URL as string;
const baseUrl = `${rootApiUrl}/api/assistants/agents`;

export async function fetchAllAgents() {
  try {
    const response = await axios.get(baseUrl);
    return response.data as Assistant[];
  } catch (error) {
    console.error('Error fetching assistants:', error);
    return null;
  }
}
