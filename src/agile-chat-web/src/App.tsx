import { BrowserRouter as Router } from 'react-router-dom';
import AppRoutes from './routing/app-routes';
import './App.css';
import { Toaster } from '@/components/ui/toaster';
import { ErrorBoundary } from './error-handling/ErrorBoundary';
import Layout from './Layout';
import { useIsAuthenticated } from '@azure/msal-react';
import { useEffect } from 'react';
import { getUserPermissions } from './services/user-service';
import { getSettings } from './services/settings-service';
import { pca } from './authentication/msal-configs';
import { useSettingsStore } from '@/stores/settings-store';

function App() {
  const isAuthenticated = useIsAuthenticated();
  const { settings } = useSettingsStore();

  useEffect(() => {
    if (isAuthenticated) {
      pca.initialize().then(() => {
        getUserPermissions();
      });
    }
  }, [isAuthenticated]);

  useEffect(() => {
    if (!isAuthenticated) return;
    if (!settings) {
      getSettings();
      return;
    }

    if (settings.faviconUrl && settings.faviconUrl !== '') {
      updateFavicon(settings.faviconUrl);
    }
    if (settings.appName && settings.appName !== '') {
      updateAppTitle(settings.appName);
    }
  }, [isAuthenticated, settings]);

  // Function to update favicon
  const updateFavicon = (faviconUrl: string) => {
    const link = document.getElementById('faviconLink') as HTMLLinkElement;
    if (link) {
      link.href = faviconUrl;
    }
  };

  const updateAppTitle = (appTitle: string) => {
    const title = document.getElementById('appTitle') as HTMLLinkElement;
    if (title) {
      title.innerHTML = appTitle;
    }
  };

  return (
    <>
      <Toaster />
      <ErrorBoundary>
        <Router>
          <Layout>
            <AppRoutes />
          </Layout>
        </Router>
      </ErrorBoundary>
    </>
  );
}

export default App;
