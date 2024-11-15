import axios from "axios";

const xApiKey = import.meta.env.VITE_XAPIKEY as string;

function getApiUrl(endpoint: string): string {
  const rootApiUrl = import.meta.env.VITE_CUSTOM_API_URL as string;
  return `${rootApiUrl}/${endpoint}`;
}

export async function IsUserSystemAdmin(
  userEmail: string
): Promise<boolean> {
  const apiUrl = getApiUrl(`rolelookup/isusersystemadmin?email=${userEmail}`);
  try {
    const response = await axios.get<boolean>(apiUrl, {
      headers: {
        "XApiKey": xApiKey,
        "Content-Type": "application/json",
      },
    });
    return response.data;
  } catch (error) {
    console.error("Error checking user role system admin:", error);
    return false;
  }
}

export async function IsUserContentManager(
  userEmail: string
): Promise<boolean> {
  const apiUrl = getApiUrl(`rolelookup/isusercontentmanager?email=${userEmail}`);
  
  try {
    const response = await axios.get<boolean>(apiUrl, {
      headers: {
        "XApiKey": xApiKey,
        "Content-Type": "application/json",
      },
    });
    return response.data;
  } catch (error) {
    console.error("Error checking user role content manager:", error);
    return false;
  }
}

export async function IsUserEndUser(
  userEmail: string
): Promise<boolean> {
  const apiUrl = getApiUrl(`rolelookup/isuserenduser?email=${userEmail}`);
  
  try {
    const response = await axios.get<boolean>(apiUrl, {
      headers: {
        "XApiKey": xApiKey,
        "Content-Type": "application/json",
      },
    });
    return response.data;
  } catch (error) {
    console.error("Error checking user role system enduser:", error);
    return false;
  }
}