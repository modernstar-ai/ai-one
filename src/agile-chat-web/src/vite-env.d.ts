/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_AGILECHAT_API_URL: string;
  readonly VITE_AGILECHAT_RAGAPI_URL: string;
  readonly VITE_AZURE_AD_CLIENT_ID: string;
  readonly VITE_AZURE_AD_CLIENT_SECRET: string;
  // more env variables...
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
