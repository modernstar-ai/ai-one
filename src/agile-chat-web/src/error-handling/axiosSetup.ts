import ax from 'axios';
import { errorHandler } from './errorHandler';
import { msalScopes, pca } from '@/authentication/msal-configs';

const axios = ax.create({});

// Add a request interceptor
axios.interceptors.request.use(
  async (config) => {
    // Get the access token
    const accounts = pca.getAllAccounts();
    if (accounts.length > 0) {
      const response = await pca.acquireTokenSilent({
        scopes: msalScopes,
        account: accounts[0],
      });

      // Set the access token in the headers
      config.headers!['Authorization'] = `Bearer ${response.accessToken}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Request interceptor
axios.interceptors.request.use(
  (config) => {
    return config;
  },
  (error) => {
    errorHandler.handleError(error);
    return Promise.reject(error);
  }
);

export default axios;
