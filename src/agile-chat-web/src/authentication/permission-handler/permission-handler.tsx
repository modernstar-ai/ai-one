import { usePermissionsStore } from '@/stores/permission-store';
import { UserRole } from '../user-roles';
import React, { ReactNode } from 'react';
import { hasPermission } from './permission-checker';

interface PermissionHandlerProps {
  children: ReactNode;
  role?: UserRole;
  group?: string;
}

export const PermissionHandler: React.FC<PermissionHandlerProps> = ({ children, ...props }) => {
  const { roles, groups } = usePermissionsStore();
  const roleRequired = props.role;
  const groupRequired = props.group;

  if (!hasPermission(roles, groups, roleRequired, groupRequired)) return null;

  return children;
};
