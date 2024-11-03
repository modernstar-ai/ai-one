import { InteractionStatus } from '@azure/msal-browser';
import { useIsAuthenticated, useMsal } from '@azure/msal-react';
import { Navigate } from 'react-router-dom';

export const ProtectedRoute = ({ children }: { children: React.ReactNode }) => {
  const { inProgress } = useMsal();

  const isAuthenticated = useIsAuthenticated();

  if (inProgress === InteractionStatus.Startup) {
    return <></>;
  } else if (!isAuthenticated) {
    // user is not authenticated
    return <Navigate to="/login" />;
  }
  return children;
};
