import { LogOut, Monitor, Moon, Sun, User } from 'lucide-react';
import { Button } from './ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger
} from './ui/dropdown-menu';
import { cn } from './ui/lib';

type Theme = 'light' | 'dark' | 'system';

interface Props {
  isMobile: boolean;
  name: string;
  username: string;
  handleLogout: () => void;
  applyTheme: (theme: Theme) => void;
}

export const UserMenu = (props: Props) => {
  const { isMobile, name, username, handleLogout, applyTheme } = props;

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button
          variant="ghost"
          size={isMobile ? 'default' : 'icon'}
          className={cn(
            'lg:justify-center justify-start',
            isMobile ? 'w-full text-gray-300 hover:text-white hover:bg-gray-800' : '',
            isMobile ? '' : 'text-gray-300 hover:text-white hover:bg-gray-800'
          )}
          aria-label="User Profile">
          <User className={cn('h-5 w-5', isMobile ? 'mr-2' : '')} />
          {isMobile && name}
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-56">
        <DropdownMenuLabel>
          <div className="flex flex-col space-y-1">
            <p className="text-sm font-medium">{name}</p>
            <p className="text-xs text-muted-foreground">{username}</p>
          </div>
        </DropdownMenuLabel>
        <DropdownMenuSeparator />

        {/* Theme Options */}
        <DropdownMenuItem onClick={() => applyTheme('light')}>
          <Sun className="mr-2 h-4 w-4" />
          <span>Light</span>
        </DropdownMenuItem>
        <DropdownMenuItem onClick={() => applyTheme('dark')}>
          <Moon className="mr-2 h-4 w-4" />
          <span>Dark</span>
        </DropdownMenuItem>
        <DropdownMenuItem onClick={() => applyTheme('system')}>
          <Monitor className="mr-2 h-4 w-4" />
          <span>System</span>
        </DropdownMenuItem>

        <DropdownMenuSeparator />

        <DropdownMenuItem onClick={handleLogout}>
          <LogOut className="mr-2 h-4 w-4" />
          <span>Log out</span>
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
};
