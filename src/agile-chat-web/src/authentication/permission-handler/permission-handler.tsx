import { usePermissionsStore } from '@/stores/permission-store';
import { UserRole } from '../user-roles';
import React, { ReactNode } from 'react';
import { hasPermission } from './permission-checker';
import { PermissionsAccessControlModel } from '@/components/ui-extended/permissions-access-control';
import { useAuth } from '@/services/auth-helpers';

interface PermissionHandlerProps {
  children: ReactNode;
  roles?: UserRole[];
  pac?: PermissionsAccessControlModel;
}

export const PermissionHandler: React.FC<PermissionHandlerProps> = ({ children, ...props }) => {
  const { roles, groups } = usePermissionsStore();
  const { username } = useAuth();

  const rolesRequired = props.roles;
  const pac = props.pac;

  if (!hasPermission(username, roles, groups, rolesRequired, pac)) return null;

  return children;
};
