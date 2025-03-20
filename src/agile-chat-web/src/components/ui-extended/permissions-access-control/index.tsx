import { Button } from '@/components/ui/button';
import { Checkbox } from '@/components/ui/checkbox';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger
} from '@/components/ui/dialog';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Textarea } from '@/components/ui/textarea';
import { LockKeyhole } from 'lucide-react';

export interface PermissionsAccessControlModel {
  contentManagers: AccessLevel;
  users: AccessLevel;
}

export interface AccessLevel {
  allowAccessToAll: boolean;
  userIds: string[];
  groups: string[];
}

interface IProps {
  pac: PermissionsAccessControlModel;
  onChange: (pac: PermissionsAccessControlModel) => void;
  hideUsers?: boolean;
}

export const permissionsAccessControlDefaultValues: PermissionsAccessControlModel = {
  contentManagers: {
    allowAccessToAll: false,
    userIds: [],
    groups: []
  },
  users: {
    allowAccessToAll: false,
    userIds: [],
    groups: []
  }
};

export const PermissionsAccessControl = (props: IProps) => {
  const { pac, onChange, hideUsers } = props;

  return (
    <Dialog>
      <DialogTrigger asChild>
        <Button>
          <LockKeyhole className="me-1" /> Access Control
        </Button>
      </DialogTrigger>
      <DialogContent className="md:min-w-[700px]">
        <DialogHeader>
          <DialogTitle>Permissions Access Control</DialogTitle>
          <DialogDescription>Manage who has access to this resource (comma or new line separated)</DialogDescription>
        </DialogHeader>

        <Tabs defaultValue={hideUsers ? 'contentManagers' : 'users'} className="flex h-full">
          <TabsList className="h-full flex flex-col justify-center items-start">
            {!hideUsers && (
              <TabsTrigger value="users" className="w-full data-[state=active]:bg-primary/25">
                Users
              </TabsTrigger>
            )}

            <TabsTrigger value="contentManagers" className="w-full data-[state=active]:bg-primary/25">
              Content Managers
            </TabsTrigger>
          </TabsList>
          {!hideUsers && (
            <TabsContent value="users" className="w-full">
              <div className="flex flex-col w-full items-center mt-0 ms-2">
                <div className="flex self-start items-center gap-2 mb-4">
                  <Checkbox
                    checked={pac.users.allowAccessToAll}
                    onCheckedChange={(e) =>
                      onChange({
                        ...pac,
                        users: {
                          ...pac.users,
                          allowAccessToAll: e.valueOf() as boolean
                        }
                      })
                    }
                  />
                  Allow access to all Users
                </div>
                <p className="self-start font-bold">Emails (case sensitive):</p>
                <Textarea
                  className="w-full"
                  defaultValue={pac.users.userIds.join('\n')}
                  onChange={(e) =>
                    onChange({
                      ...pac,
                      users: {
                        ...pac.users,
                        userIds: e.target.value.split(/[,\n]+/).filter((x) => x.length > 0)
                      }
                    })
                  }
                />

                <p className="self-start font-bold">Groups (case sensitive):</p>
                <Textarea
                  className="w-full"
                  defaultValue={pac.users.groups.join('\n')}
                  onChange={(e) =>
                    onChange({
                      ...pac,
                      users: {
                        ...pac.users,
                        groups: e.target.value.split(/[,\n]+/).filter((x) => x.length > 0)
                      }
                    })
                  }
                />
              </div>
            </TabsContent>
          )}

          <TabsContent value="contentManagers" className="w-full">
            <div className="flex flex-col w-full items-center mt-0 ms-2">
              <div className="flex self-start items-center gap-2 mb-4">
                <Checkbox
                  checked={pac.contentManagers.allowAccessToAll}
                  onCheckedChange={(e) =>
                    onChange({
                      ...pac,
                      contentManagers: {
                        ...pac.contentManagers,
                        allowAccessToAll: e.valueOf() as boolean
                      }
                    })
                  }
                />
                Allow access to all Content Managers
              </div>
              <p className="self-start font-bold">Emails (case sensitive):</p>
              <Textarea
                className="w-full"
                defaultValue={pac.contentManagers.userIds.join('\n')}
                onChange={(e) =>
                  onChange({
                    ...pac,
                    contentManagers: {
                      ...pac.contentManagers,
                      userIds: e.target.value.split(/[,\n]+/).filter((x) => x.length > 0)
                    }
                  })
                }
              />

              <p className="self-start font-bold">Groups (case sensitive):</p>
              <Textarea
                className="w-full"
                defaultValue={pac.contentManagers.groups.join('\n')}
                onChange={(e) =>
                  onChange({
                    ...pac,
                    contentManagers: {
                      ...pac.contentManagers,
                      groups: e.target.value.split(/[,\n]+/).filter((x) => x.length > 0)
                    }
                  })
                }
              />
            </div>
          </TabsContent>
        </Tabs>
      </DialogContent>
    </Dialog>
  );
};
