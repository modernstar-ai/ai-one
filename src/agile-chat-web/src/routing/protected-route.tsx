import { hasPermission } from '@/authentication/permission-handler/permission-checker';
import { UserRole } from '@/authentication/user-roles';
import { useAuth } from '@/services/auth-helpers';
import { usePermissionsStore } from '@/stores/permission-store';
import { InteractionStatus } from '@azure/msal-browser';
import { useIsAuthenticated, useMsal } from '@azure/msal-react';
import { Loader2Icon } from 'lucide-react';
import { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';

interface ProtectedRouteProps {
  children: ReactNode;
  role?: UserRole;
}
export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children, role }) => {
  const { roles } = usePermissionsStore();
  const { inProgress } = useMsal();

  const isAuthenticated = useIsAuthenticated();
  const { username } = useAuth();

  if (isAuthenticated && roles && role && !hasPermission(username, roles, [], [role])) {
    return <Navigate to="/" />;
  }

  if (inProgress === InteractionStatus.Startup) {
    return <></>;
  } else if (!isAuthenticated) {
    // user is not authenticated
    return <Navigate to="/login" />;
  }

  if (!roles) {
    return (
      <div className="flex w-full h-full items-center justify-center">
        <Loader2Icon className="animate-spin" />
      </div>
    );
  }

  return children;
};
