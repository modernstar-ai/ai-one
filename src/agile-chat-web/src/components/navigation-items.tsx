import { PermissionHandler } from '@/authentication/permission-handler/permission-handler';
import { UserRole } from '@/authentication/user-roles';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';
import { Home, Bot, FileBox, Database } from 'lucide-react';

interface Props {
  isMobile?: boolean;
  handleNavigation: (path: string) => void;
}

export const NavigationItems = ({ isMobile = false, handleNavigation }: Props) => (
  <div className={cn('flex space-y-2', isMobile ? 'flex-col' : 'flex-col mt-4')}>
    <Button
      variant="ghost"
      size={isMobile ? 'default' : 'icon'}
      className={cn(
        'justify-start',
        isMobile ? 'w-full text-gray-300 hover:text-white hover:bg-gray-800' : '',
        isMobile ? '' : 'text-gray-300 hover:text-white hover:bg-gray-800'
      )}
      onClick={() => handleNavigation('/')}>
      <Home className={cn('h-5 w-5', isMobile ? 'mr-2' : '')} />
      {isMobile && 'Home'}
    </Button>

    <Button
      variant="ghost"
      size={isMobile ? 'default' : 'icon'}
      className={cn(
        'justify-start',
        isMobile ? 'w-full text-gray-300 hover:text-white hover:bg-gray-800' : '',
        isMobile ? '' : 'text-gray-300 hover:text-white hover:bg-gray-800'
      )}
      onClick={() => handleNavigation('/assistants')}>
      <Bot className={cn('h-5 w-5', isMobile ? 'mr-2' : '')} />
      {isMobile && 'Assistants'}
    </Button>

    <PermissionHandler roles={[UserRole.ContentManager]}>
      <Button
        variant="ghost"
        size={isMobile ? 'default' : 'icon'}
        className={cn(
          'justify-start',
          isMobile ? 'w-full text-gray-300 hover:text-white hover:bg-gray-800' : '',
          isMobile ? '' : 'text-gray-300 hover:text-white hover:bg-gray-800'
        )}
        onClick={() => handleNavigation('/files')}>
        <FileBox className={cn('h-5 w-5', isMobile ? 'mr-2' : '')} />
        {isMobile && 'Files'}
      </Button>
    </PermissionHandler>

    <PermissionHandler roles={[UserRole.ContentManager]}>
      <Button
        variant="ghost"
        size={isMobile ? 'default' : 'icon'}
        className={cn(
          'justify-start',
          isMobile ? 'w-full text-gray-300 hover:text-white hover:bg-gray-800' : '',
          isMobile ? '' : 'text-gray-300 hover:text-white hover:bg-gray-800'
        )}
        onClick={() => handleNavigation('/containers')}>
        <Database className={cn('h-5 w-5', isMobile ? 'mr-2' : '')} />
        {isMobile && 'Database'}
      </Button>
    </PermissionHandler>
  </div>
);
