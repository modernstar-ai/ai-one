import { createRoot } from 'react-dom/client';
import App from './App.tsx';
import './global.css';
import { MsalProvider } from '@azure/msal-react';
import { pca } from './authentication/msal-configs.ts';

createRoot(document.getElementById('root')!).render(
  <MsalProvider instance={pca}>
    <App />
  </MsalProvider>
);
