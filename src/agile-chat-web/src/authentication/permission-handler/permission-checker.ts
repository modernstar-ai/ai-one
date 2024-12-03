import { UserRole } from "../user-roles";

export const hasPermission = (
  assignedRoles: string[] | undefined,
  assignedGroups: string[] | undefined,
  roleRequired?: UserRole,
  groupRequired?: string,
  blockContentManagerIfNoGroup: boolean = false
): boolean => {
  if (assignedRoles === undefined || assignedGroups === undefined) return false;

  //If you are SystemAdmin just always return true
  if (assignedRoles.includes(UserRole.SystemAdmin)) return true;

  //If both roles and groups are needed, check if the user has both
  if (
    roleRequired &&
    groupRequired &&
    groupRequired.length > 0 &&
    !assignedRoles.includes(roleRequired + `.${groupRequired.toLowerCase()}`)
  ) {
    return false;
  }

  //If only a role is required, check if the user has ContentManager in any of their roles
  if (roleRequired) {
    if (roleRequired === UserRole.SystemAdmin && !assignedRoles?.includes(UserRole.SystemAdmin)) return false;
    if (
      (roleRequired === UserRole.ContentManager &&
        assignedRoles?.filter((role) => role.startsWith(UserRole.ContentManager)).length === 0) ||
      (!groupRequired && blockContentManagerIfNoGroup)
    )
    return false;
  }

  //If only a group is required, check if the users groups contains that group
  if (groupRequired && groupRequired.length > 0) {
    if (!groupRequired?.includes(groupRequired.toLowerCase())) return false;
  }

  return true;
};
