import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import App from './App.tsx';
import './global.css';
import { MsalProvider } from '@azure/msal-react';
import { pca } from './msal-configs.ts';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <MsalProvider instance={pca}>
      <App />
    </MsalProvider>
  </StrictMode>
);
