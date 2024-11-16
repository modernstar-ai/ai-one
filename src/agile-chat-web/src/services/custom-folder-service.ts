import axios from "axios";

const xApiKey = import.meta.env.VITE_XAPIKEY as string;

interface Folder {
  folder: string;
}

function getApiUrl(endpoint: string): string {
  const rootApiUrl = import.meta.env.VITE_CUSTOM_API_URL as string;
  return `${rootApiUrl}/${endpoint}`;
}

export async function fetchManageableFolders(
  userEmail: string
): Promise<Folder[] | null> {
  const apiUrl = getApiUrl(`folderlookup/getmanageablefolders?userEmail=${userEmail}`);
  
  try {
    const response = await axios.get<string[]>(apiUrl, {
      headers: {
        "XApiKey": xApiKey,
        "Content-Type": "application/json",
      },
    });
    console.log("API response data:", response.data);
    return response.data.map((item) => ({ folder: item }));
  } catch (error) {
    console.error("Error fetching folders:", error);
    return null;
  }
}

export async function fetchAccessibleFolders(
  userEmail: string
): Promise<Folder[] | null> {
  const apiUrl = getApiUrl(`folderlookup/getaccessiblefolders?userEmail=${userEmail}`);
  
  try {
    const response = await axios.get<string[]>(apiUrl, {
      headers: {
        "XApiKey": xApiKey,
        "Content-Type": "application/json",
      },
    });
    return response.data.map((item) => ({ folder: item }));
  } catch (error) {
    console.error("Error fetching folders:", error);
    return null;
  }
}

export async function fetchAllFolders(): Promise<Folder[] | null> {
  const apiUrl = getApiUrl(`folderlookup/getallfolders`);
  try {
    const response = await axios.get<string[]>(apiUrl, {
      headers: {
        "XApiKey": xApiKey,
        "Content-Type": "application/json",
      },
    });
    return response.data.map((item) => ({ folder: item }));
  } catch (error) {
    console.error("Error fetching folders:", error);
    return null;
  }
}