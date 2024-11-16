import axios from "axios";

interface Group {
  group: string;
}
const xApiKey = import.meta.env.VITE_XAPIKEY as string;

function getApiUrl(endpoint: string): string {
  const rootApiUrl = import.meta.env.VITE_CUSTOM_API_URL as string;
  return `${rootApiUrl}/${endpoint}`;
}

export async function fetchManageableGroups(
  userEmail: string
): Promise<Group[] | null> {
  const apiUrl = getApiUrl(`grouplookup/getmanageablegroups?userEmail=${userEmail}`);
  
  try {
    const response = await axios.get<string[]>(apiUrl, {
      headers: {
        "XApiKey": xApiKey,
        "Content-Type": "application/json",
      },
    });
    return response.data.map((item) => ({ group: item }));
  } catch (error) {
    console.error("Error fetching groups:", error);
    return null;
  }
}

export async function fetchAccessibileGroups(
  userEmail: string
): Promise<Group[] | null> {
  const apiUrl = getApiUrl(`grouplookup/getaccessiblegroups?userEmail=${userEmail}`);
  
  try {
    const response = await axios.get<string[]>(apiUrl, {
      headers: {
        "XApiKey": xApiKey,
        "Content-Type": "application/json",
      },
    });
    return response.data.map((item) => ({ group: item }));
  } catch (error) {
    console.error("Error fetching groups:", error);
    return null;
  }
}

export async function IsGroupValid(
  group: string
): Promise<boolean> {
  const apiUrl = getApiUrl(`grouplookup/isgroupvalid?group=${group}`);
  
  try {
    const response = await axios.get<boolean>(apiUrl, {
      headers: {
        "XApiKey": xApiKey,
        "Content-Type": "application/json",
      },
    });
    return response.data;
  } catch (error) {
    console.error("Error checking group validity:", error);
    return false;
  }
}