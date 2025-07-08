// src/services/personaservice.ts
import axios from '@/error-handling/axiosSetup';
import { PermissionsState, usePermissionsStore } from '@/stores/permission-store';

function getApiUrl(endpoint: string = ''): string {
  const rootApiUrl = import.meta.env.VITE_AGILECHAT_API_URL as string;
  return `${rootApiUrl}/api/users/${endpoint}`;
}

export async function getUserPermissions(): Promise<void> {
  const apiUrl = getApiUrl('permissions');

  try {
    const response = await axios.get<PermissionsState>(apiUrl);
    usePermissionsStore.setState({
      roles: response.data.roles ?? [],
      groups: response.data.groups ?? [],
    });
  } catch (error) {
    console.error('Error fetching permissions:', error);
  }
}
