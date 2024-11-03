import { BrowserRouter as Router } from 'react-router-dom';
import AppRoutes from './routing/app-routes';
import './App.css';
import { Toaster } from '@/components/ui/toaster';
import { ErrorBoundary } from './error-handling/ErrorBoundary';
import Layout from './Layout';

function App() {
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
