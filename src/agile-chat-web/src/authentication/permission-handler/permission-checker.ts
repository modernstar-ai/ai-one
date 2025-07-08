import { PermissionsAccessControlModel } from '@/components/ui-extended/permissions-access-control';
import { UserRole } from '../user-roles';

export const hasPermission = (
  username: string,
  assignedRoles: string[] | undefined,
  assignedGroups: string[] | undefined,
  rolesRequired?: UserRole[],
  pac?: PermissionsAccessControlModel
): boolean => {
  if (assignedRoles === undefined || assignedGroups === undefined) return false;

  //If you are SystemAdmin just always return true
  if (assignedRoles.includes(UserRole.SystemAdmin)) return true;

  //Check if roles required are satisfied
  if (!rolesRequired?.some((role) => assignedRoles.includes(role))) return false;

  //Check if pac is satisfied
  if (!pac) return true;

  if (
    pac.users.allowAccessToAll ||
    pac.users.userIds.includes(username) ||
    pac.contentManagers.userIds.includes(username) ||
    pac.users.groups.some((group) => assignedGroups.includes(group)) ||
    pac.contentManagers.groups.some((group) => assignedGroups.includes(group)) ||
    (assignedRoles.includes(UserRole.ContentManager) && pac.contentManagers.allowAccessToAll)
  )
    return true;

  return false;
};
