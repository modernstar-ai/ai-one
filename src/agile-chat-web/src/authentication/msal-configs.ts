import { Configuration, PublicClientApplication } from '@azure/msal-browser';

const configuration: Configuration = {
  auth: {
    clientId: import.meta.env.VITE_AZURE_AD_CLIENT_ID,
    authority: `https://login.microsoftonline.com/${import.meta.env.VITE_AZURE_AD_TENANT_ID}`,
    redirectUri: window.location.origin,
  },
  cache: {
    cacheLocation: 'localStorage',
    storeAuthStateInCookie: true,
  },
};

export const pca = new PublicClientApplication(configuration);
export const msalScopes = [`api://${import.meta.env.VITE_AZURE_AD_CLIENT_ID}/User.Read`];
