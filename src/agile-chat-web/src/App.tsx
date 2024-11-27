import { BrowserRouter as Router } from 'react-router-dom';
import AppRoutes from './routing/app-routes';
import './App.css';
import { Toaster } from '@/components/ui/toaster';
import { ErrorBoundary } from './error-handling/ErrorBoundary';
import Layout from './Layout';
import { useIsAuthenticated } from '@azure/msal-react';
import { useEffect } from 'react';
import { getUserPermissions } from './services/user-service';

function App() {
  const isAuthenticated = useIsAuthenticated();
  useEffect(() => {
    if (isAuthenticated) {
      getUserPermissions();
    }
  }, [isAuthenticated]);

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
