import { z } from 'zod';

export const AccessControlSchema = z.object({
  allowAccessToAll: z.boolean(),
  userIds: z.array(z.string()),
  groups: z.array(z.string())
});

export const PermissionsAccessControlSchema = z.object({
  contentManagers: AccessControlSchema,
  users: AccessControlSchema
});
