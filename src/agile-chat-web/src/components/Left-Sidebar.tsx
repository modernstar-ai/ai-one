import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

import { useAuth } from '@/services/auth-helpers';
import { cn } from '@/lib/utils';
import { createChatThread, deleteChatThread } from '@/services/chatthreadservice';

import { Button } from '@/components/ui/button';

import { ScrollArea } from '@/components/ui/scroll-area';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger
} from '@/components/ui/alert-dialog';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger
} from '@/components/ui/dropdown-menu';
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '@/components/ui/tooltip';
import { Sheet, SheetContent, SheetTrigger } from '@/components/ui/sheet';

import {
  Home,
  FileBox,
  User,
  LogOut,
  Plus,
  Trash2,
  Sun,
  Moon,
  Monitor,
  PanelLeftOpen,
  PanelLeftClose,
  Loader2,
  Database,
  Bot,
  Menu
} from 'lucide-react';
import { PermissionHandler } from '@/authentication/permission-handler/permission-handler';
import { UserRole } from '@/authentication/user-roles';

import Logo from '@/assets/agile-logo.png';
import { useThreadsStore } from '@/stores/threads-store';
import { useSettingsStore } from '@/stores/settings-store';

type Theme = 'light' | 'dark' | 'system';

export function ResponsiveNavigation() {
  const { threads, refreshThreads } = useThreadsStore();
  const [isPanelOpen, setIsPanelOpen] = useState(false);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [loading, setLoading] = useState(false);
  const [initialLoad, setInitialLoad] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();
  const { instance, accounts, isLoggedIn, name, username } = useAuth();
  const { settings } = useSettingsStore();

  // Theme handling
  useEffect(() => {
    const savedTheme = (localStorage.getItem('theme') as Theme) || 'system';
    applyTheme(savedTheme);
  }, []);

  const applyTheme = (theme: Theme) => {
    localStorage.setItem('theme', theme);

    if (theme === 'system') {
      if (window.matchMedia('(prefers-color-scheme: dark)').matches) {
        document.documentElement.classList.add('dark');
      } else {
        document.documentElement.classList.remove('dark');
      }
    } else if (theme === 'dark') {
      document.documentElement.classList.add('dark');
    } else {
      document.documentElement.classList.remove('dark');
    }
  };

  // Watch for system theme changes
  useEffect(() => {
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    const handleChange = () => {
      if (localStorage.getItem('theme') === 'system') {
        applyTheme('system');
      }
    };

    mediaQuery.addEventListener('change', handleChange);
    return () => mediaQuery.removeEventListener('change', handleChange);
  }, []);

  // Initial load
  useEffect(() => {
    if (isLoggedIn && username && isPanelOpen) {
      loadChatThreads(true);
    }
  }, [isPanelOpen]);

  // Load when panel is opened
  useEffect(() => {
    if (isPanelOpen && isLoggedIn && username && !initialLoad) {
      loadChatThreads(false);
    }
  }, [isPanelOpen]);

  // Periodic refresh when panel is open
  useEffect(() => {
    let intervalId: NodeJS.Timeout;

    if (isPanelOpen && isLoggedIn && username) {
      intervalId = setInterval(() => loadChatThreads(false), 30000);
    }

    return () => {
      if (intervalId) {
        clearInterval(intervalId);
      }
    };
  }, [isPanelOpen, isLoggedIn, username]);

  const loadChatThreads = async (isInitial: boolean = false) => {
    if (isInitial) {
      setInitialLoad(true);
    }
    setLoading(true);
    setError(null);

    await refreshThreads();
    setLoading(false);
  };

  const handleCreateChat = async () => {
    setLoading(true);
    try {
      // Get assistantId from query string if it exists
      const urlParams = new URLSearchParams(window.location.search);
      const assistantId = urlParams.get('assistantId');

      // Prepare chat thread data
      const chatData = {
        name: 'New Chat',
        personaMessage: '',
        personaMessageTitle: '',
        userId: username,
        ...(assistantId && { assistantId }) // Only add assistantId if it exists
      };

      const newThread = await createChatThread(chatData);

      if (newThread) {
        await loadChatThreads(false);

        // Navigate to new chat
        navigate(`/chat/${newThread.id}`);
        setIsMobileMenuOpen(false); // Close mobile menu after navigation
      } else {
        setError('Failed to create new chat');
      }
    } catch {
      setError('Failed to create new chat');
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteThread = async (threadId: string) => {
    setLoading(true);
    try {
      const success = await deleteChatThread(threadId);
      if (success) {
        await loadChatThreads(false);
      } else {
        setError('Failed to delete chat thread');
      }
    } catch {
      setError('Failed to delete chat thread');
    } finally {
      setLoading(false);
    }
  };

  const handleClearHistory = async () => {
    const confirmClear = true;
    if (confirmClear) {
      setLoading(true);
      try {
        const deletePromises = threads?.map((thread) => deleteChatThread(thread.id));
        await Promise.all(deletePromises ?? []);
        await loadChatThreads(false);
      } catch {
        setError('Failed to clear chat history');
      } finally {
        setLoading(false);
      }
    }
  };

  const handleLogout = () => {
    instance.logoutRedirect({ account: accounts[0] }).then(() => {
      instance.logoutPopup({ account: accounts[0] });
    });
  };

  const handleNavigation = (path: string) => {
    navigate(path);
    setIsMobileMenuOpen(false);
  };

  // Navigation items component for reuse
  const NavigationItems = ({ isMobile = false }: { isMobile?: boolean }) => (
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

  // User menu component for reuse
  const UserMenu = ({ isMobile = false }: { isMobile?: boolean }) => (
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
  // Chat threads component for reuse
  const ChatThreads = () => (
    <div className="flex flex-col h-full">
      <div className="p-4 border-b border-gray-700 flex justify-between items-center">
        <h2 className="font-semibold text-white">Recent Chats</h2>
        <Button
          variant="ghost"
          size="icon"
          onClick={handleCreateChat}
          disabled={loading}
          aria-label="New Chat"
          className="text-gray-300 hover:text-white hover:bg-gray-800">
          {loading ? <Loader2 className="h-5 w-5 animate-spin" /> : <Plus className="h-5 w-5" />}
        </Button>
      </div>

      {/* Error Message */}
      {error && <div className="px-4 py-2 text-red-400 text-sm">{error}</div>}

      {/* Chat Threads */}
      {loading && threads?.length === 0 ? (
        <div className="flex-1 flex items-center justify-center">
          <Loader2 className="h-8 w-8 animate-spin text-gray-400" />
        </div>
      ) : (
        <ScrollArea className="flex-1">
          <div className="p-4 space-y-2">
            {threads?.map((thread) => (
              <div
                key={thread.id}
                className={cn(
                  'group flex items-center justify-between p-2 rounded-md',
                  'hover:bg-gray-800 cursor-pointer',
                  loading && 'opacity-50 pointer-events-none'
                )}>
                <div
                  className="flex flex-col flex-grow min-w-0"
                  onClick={() => {
                    navigate(`/chat/${thread.id}`);
                    setIsMobileMenuOpen(false);
                  }}>
                  <span className="text-sm font-medium truncate text-gray-200">
                    {thread.name.length > 50 ? `${thread.name.slice(0, 30)}...` : thread.name}
                  </span>
                </div>
                <AlertDialog>
                  <AlertDialogTrigger asChild>
                    <Button
                      variant="ghost"
                      size="icon"
                      className="opacity-0 group-hover:opacity-100 text-gray-400 hover:text-red-400 hover:bg-gray-800"
                      disabled={loading}
                      onClick={(e) => e.stopPropagation()}>
                      {loading ? <Loader2 className="h-4 w-4 animate-spin" /> : <Trash2 className="h-4 w-4" />}
                    </Button>
                  </AlertDialogTrigger>
                  <AlertDialogContent>
                    <AlertDialogHeader>
                      <AlertDialogTitle>Delete Chat</AlertDialogTitle>
                      <AlertDialogDescription>
                        Are you sure you want to delete this chat? This action cannot be undone.
                      </AlertDialogDescription>
                    </AlertDialogHeader>
                    <AlertDialogFooter>
                      <AlertDialogCancel>Cancel</AlertDialogCancel>
                      <AlertDialogAction
                        onClick={() => handleDeleteThread(thread.id)}
                        className="bg-destructive text-destructive-foreground hover:bg-destructive/90">
                        Delete
                      </AlertDialogAction>
                    </AlertDialogFooter>
                  </AlertDialogContent>
                </AlertDialog>
              </div>
            ))}
          </div>
        </ScrollArea>
      )}

      {/* Clear History Button */}
      {threads && threads.length > 0 && (
        <div className="p-4 border-t border-gray-700">
          <AlertDialog>
            <AlertDialogTrigger asChild>
              <Button
                variant="outline"
                className="w-full border-gray-600 bg-gray-900 text-gray-300 hover:bg-gray-800 hover:text-white"
                disabled={loading}>
                {loading ? <Loader2 className="h-4 w-4 animate-spin mr-2" /> : null}
                Clear History
              </Button>
            </AlertDialogTrigger>
            <AlertDialogContent>
              <AlertDialogHeader>
                <AlertDialogTitle>Clear All Chat History</AlertDialogTitle>
                <AlertDialogDescription>
                  Are you sure you want to clear all chat history? This action cannot be undone and will delete all your
                  conversations.
                </AlertDialogDescription>
              </AlertDialogHeader>
              <AlertDialogFooter>
                <AlertDialogCancel>Cancel</AlertDialogCancel>
                <AlertDialogAction
                  onClick={handleClearHistory}
                  className="bg-destructive text-destructive-foreground hover:bg-destructive/90">
                  Clear All
                </AlertDialogAction>
              </AlertDialogFooter>
            </AlertDialogContent>
          </AlertDialog>
        </div>
      )}
    </div>
  );

  return (
    <div className="h-screen flex flex-col">
      {/* Top Bar for Mobile */}
      <div className="lg:hidden flex items-center justify-between p-4 border-b bg-gray-900 border-gray-700 fixed w-full h-[5vh]">
        <div className="flex items-center space-x-2">
          <img
            src={settings?.logoUrl && settings.logoUrl != '' ? settings.logoUrl : Logo}
            alt="Agile Logo"
            className="h-8 w-auto"
          />
        </div>

        <div className="flex items-center space-x-2">
          <Button
            variant="ghost"
            size="icon"
            onClick={handleCreateChat}
            disabled={loading}
            aria-label="New Chat"
            className="text-gray-300 hover:text-white hover:bg-gray-800">
            {loading ? <Loader2 className="h-5 w-5 animate-spin" /> : <Plus className="h-5 w-5" />}
          </Button>

          <Sheet open={isMobileMenuOpen} onOpenChange={setIsMobileMenuOpen}>
            <SheetTrigger asChild>
              <Button
                variant="ghost"
                size="icon"
                aria-label="Menu"
                className="text-gray-300 hover:text-white hover:bg-gray-800">
                <Menu className="h-5 w-5" />
              </Button>
            </SheetTrigger>
            <SheetContent side="left" className="w-80 p-0 bg-gray-900 border-gray-700">
              <div className="flex flex-col h-full">
                <div className="p-4 border-b border-gray-700">
                  <img
                    src={settings?.logoUrl && settings.logoUrl != '' ? settings.logoUrl : Logo}
                    alt="Agile Logo"
                    className="w-1/2 mb-4"
                  />
                  <div className="space-y-2">
                    <NavigationItems isMobile={true} />
                    {isLoggedIn && <UserMenu isMobile={true} />}
                  </div>
                </div>

                <div className="flex-1 overflow-hidden">
                  <ChatThreads />
                </div>
              </div>
            </SheetContent>
          </Sheet>
        </div>
      </div>

      {/* Desktop Layout */}
      <div className="hidden lg:flex flex-1 sticky">
        {/* Fixed Sidebar - Always Dark */}
        <div className="w-16 flex flex-col items-center py-4 border-r bg-gray-900 border-gray-700">
          <TooltipProvider>
            {/* Home and Panel Toggle Buttons */}
            <div className="flex flex-col space-y-2">
              <Tooltip>
                <TooltipTrigger asChild>
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => navigate('/')}
                    aria-label="Home"
                    className="text-gray-300 hover:text-white hover:bg-gray-800">
                    <Home className="h-5 w-5" />
                  </Button>
                </TooltipTrigger>
                <TooltipContent side="right">Home</TooltipContent>
              </Tooltip>

              {/* Panel Toggle Button */}
              <Tooltip>
                <TooltipTrigger asChild>
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => setIsPanelOpen(!isPanelOpen)}
                    aria-label="Toggle Panel"
                    className="text-gray-300 hover:text-white hover:bg-gray-800">
                    {isPanelOpen ? <PanelLeftClose className="h-5 w-5" /> : <PanelLeftOpen className="h-5 w-5" />}
                  </Button>
                </TooltipTrigger>
                <TooltipContent side="right">{isPanelOpen ? 'Hide Recent Chats' : 'Show Recent Chats'}</TooltipContent>
              </Tooltip>
            </div>

            {/* Navigation Items */}
            <div className="flex flex-col space-y-2 mt-4 flex-1 justify-center">
              <Tooltip>
                <TooltipTrigger asChild>
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => navigate('/assistants')}
                    aria-label="Assistants"
                    className="text-gray-300 hover:text-white hover:bg-gray-800">
                    <Bot className="h-5 w-5" />
                  </Button>
                </TooltipTrigger>
                <TooltipContent side="right">Assistants</TooltipContent>
              </Tooltip>

              <PermissionHandler roles={[UserRole.ContentManager]}>
                <Tooltip>
                  <TooltipTrigger asChild>
                    <Button
                      variant="ghost"
                      size="icon"
                      onClick={() => navigate('/files')}
                      aria-label="Files"
                      className="text-gray-300 hover:text-white hover:bg-gray-800">
                      <FileBox className="h-5 w-5" />
                    </Button>
                  </TooltipTrigger>
                  <TooltipContent side="right">Files</TooltipContent>
                </Tooltip>
              </PermissionHandler>

              <PermissionHandler roles={[UserRole.ContentManager]}>
                <Tooltip>
                  <TooltipTrigger asChild>
                    <Button
                      variant="ghost"
                      size="icon"
                      onClick={() => navigate('/containers')}
                      aria-label="Database"
                      className="text-gray-300 hover:text-white hover:bg-gray-800">
                      <Database className="h-5 w-5" />
                    </Button>
                  </TooltipTrigger>
                  <TooltipContent side="right">Database</TooltipContent>
                </Tooltip>
              </PermissionHandler>
            </div>

            {isLoggedIn && (
              <div className="mt-auto flex justify-center items-center">
                <UserMenu />
              </div>
            )}
          </TooltipProvider>
        </div>

        {/* Expandable Panel - Always Dark */}
        <div
          className={cn(
            'border-r bg-gray-900 border-gray-700 transition-all duration-300 ease-in-out',
            isPanelOpen ? 'w-80' : 'w-0 opacity-0'
          )}>
          {isPanelOpen && (
            <div className="h-full flex flex-col lg:max-h-screen">
              <div className="p-4 space-y-2">
                <img
                  src={settings?.logoUrl && settings.logoUrl != '' ? settings.logoUrl : Logo}
                  alt="Agile Logo"
                  className="w-1/2"
                />
              </div>

              <div className="flex-1 overflow-hidden">
                <ChatThreads />
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
