import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

import { useAuth } from '@/services/auth-helpers';
import { cn } from '@/lib/utils';
import { createChatThread, deleteChatThread } from '@/services/chatthreadservice';

import { Button } from '@/components/ui/button';
import SideNavButton from '@/components/navigation/left-sidebar-button';

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
  AlertDialogTrigger,
} from '@/components/ui/alert-dialog';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '@/components/ui/tooltip';

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
} from 'lucide-react';
import { PermissionHandler } from '@/authentication/permission-handler/permission-handler';
import { UserRole } from '@/authentication/user-roles';

import Logo from '@/assets/agile-logo.png';
import { useThreadsStore } from '@/stores/threads-store';

type Theme = 'light' | 'dark' | 'system';

export function LeftSidebar() {
  const { threads, refreshThreads } = useThreadsStore();
  const [isPanelOpen, setIsPanelOpen] = useState(false);
  const [loading, setLoading] = useState(false);
  const [initialLoad, setInitialLoad] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();
  const { instance, accounts, isLoggedIn, name, username } = useAuth();

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
        ...(assistantId && { assistantId }), // Only add assistantId if it exists
      };

      const newThread = await createChatThread(chatData);

      if (newThread) {
        await loadChatThreads(false);

        // Navigate to new chat
        navigate(`/chat/${newThread.id}`);
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

  return (
    <div className="flex h-screen dark">
      {/* Fixed Sidebar */}
      <div className="w-16 flex flex-col items-center py-4 border-r bg-background ">
        <TooltipProvider>
          {/* Home and Panel Toggle Buttons */}
          <div className="flex flex-col space-y-2 dark:text-white">
            <SideNavButton path="/" label="Home" Icon={Home} accessKey="h" />

            {/* Panel Toggle Button */}
            <Tooltip>
              <TooltipTrigger asChild>
                <Button variant="ghost" size="icon" aria-label="Home" onClick={() => setIsPanelOpen(!isPanelOpen)}>
                  {isPanelOpen ? <PanelLeftClose className="h-5 w-5" /> : <PanelLeftOpen className="h-5 w-5" />}
                </Button>
              </TooltipTrigger>
              <TooltipContent side="right">{isPanelOpen ? 'Hide Recent Chats' : 'Show Recent Chats'}</TooltipContent>
            </Tooltip>
          </div>

          {/* Navigation Items */}
          <div className="flex flex-col space-y-2 mt-4 h-screen justify-center items-center  dark:text-white">
            <SideNavButton path="/assistants" label="Assistants" Icon={Bot} accessKey="a" />
            <PermissionHandler role={UserRole.ContentManager}>
              <SideNavButton path="/files" label="Files" Icon={FileBox} accessKey="U" />
            </PermissionHandler>
            <PermissionHandler role={UserRole.ContentManager}>
              <SideNavButton path="/containers" label="Database" Icon={Database} accessKey="c" />
            </PermissionHandler>
            {/* <PermissionHandler role={UserRole.SystemAdmin}>
              <SideNavButton path="/tools" label="Tools" Icon={Wrench} accessKey="t" />
            </PermissionHandler> */}
          </div>

          {/* User Menu */}
          {isLoggedIn && (
            <div className="mt-auto dark:text-white dark">
              <DropdownMenu data-theme="dark">
                <DropdownMenuTrigger asChild>
                  <Button variant="ghost" size="icon" aria-label="User Profile">
                    <User className="h-5 w-5" />
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" className="w-56  dark text-white">
                  <DropdownMenuLabel ata-theme="dark">
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
            </div>
          )}
        </TooltipProvider>
      </div>

      {/* Collapsible Panel */}
      <div
        className={cn(
          'border-r bg-background transition-all duration-300 ease-in-out',
          isPanelOpen ? 'w-80' : 'w-0 opacity-0'
        )}
      >
        {isPanelOpen && (
          <div className="h-full flex flex-col dark:text-white ">
            {/* Panel Header */}
            <div className="p-4 space-y-2">
              <img src={Logo} alt="Agile Logo" className="w-1/2" />
            </div>

            <div className="p-4 border-b flex justify-between">
              <h2 className="font-semibold">Recent Chats</h2>

              <Button variant="ghost" size="icon" onClick={handleCreateChat} disabled={loading} aria-label="New Chat">
                {loading ? <Loader2 className="h-5 w-5 animate-spin" /> : <Plus className="h-5 w-5" />}
              </Button>
            </div>

            {/* Error Message */}
            {error && <div className="px-4 py-2 text-destructive text-sm">{error}</div>}

            {/* Chat Threads */}
            {loading && threads?.length === 0 ? (
              <div className="flex-1 flex items-center justify-center">
                <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
              </div>
            ) : (
              <ScrollArea className="flex-1">
                <div className="p-4 space-y-2">
                  {threads?.map((thread) => (
                    <div
                      key={thread.id}
                      className={cn(
                        'group flex items-center justify-between p-2 rounded-md',
                        'hover:bg-muted cursor-pointer',
                        loading && 'opacity-50 pointer-events-none'
                      )}
                    >
                      <div
                        className="flex flex-col flex-grow min-w-0"
                        // onClick={() => navigate(`/chat/${thread.id}`)}
                        onClick={() => navigate(`/chat/${thread.id}`)}
                      >
                        <span className="text-sm font-medium truncate">
                          {thread.name.length > 50 ? `${thread.name.slice(0, 30)}...` : thread.name}
                        </span>
                        {/* <span className="text-xs text-muted-foreground">
                          {new Date(thread.lastMessageAt).toLocaleDateString()}
                        </span> */}
                      </div>
                      <AlertDialog>
                        <AlertDialogTrigger asChild>
                          <Button
                            variant="ghost"
                            size="icon"
                            className="opacity-0 group-hover:opacity-100"
                            disabled={loading}
                            onClick={(e) => e.stopPropagation()}
                          >
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
                              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
                            >
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
              <div className="p-4 border-t">
                <AlertDialog>
                  <AlertDialogTrigger asChild>
                    <Button variant="outline" className="w-full" disabled={loading}>
                      {loading ? <Loader2 className="h-4 w-4 animate-spin mr-2" /> : null}
                      Clear History
                    </Button>
                  </AlertDialogTrigger>
                  <AlertDialogContent>
                    <AlertDialogHeader>
                      <AlertDialogTitle>Clear All Chat History</AlertDialogTitle>
                      <AlertDialogDescription>
                        Are you sure you want to clear all chat history? This action cannot be undone and will delete
                        all your conversations.
                      </AlertDialogDescription>
                    </AlertDialogHeader>
                    <AlertDialogFooter>
                      <AlertDialogCancel>Cancel</AlertDialogCancel>
                      <AlertDialogAction
                        onClick={handleClearHistory}
                        className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
                      >
                        Clear All
                      </AlertDialogAction>
                    </AlertDialogFooter>
                  </AlertDialogContent>
                </AlertDialog>
              </div>
            )}
          </div>
        )}
      </div>

      {/* Optional: Add an overlay for mobile when panel is open */}
      {isPanelOpen && (
        <div
          className="lg:hidden fixed inset-0 bg-background/80 backdrop-blur-sm"
          onClick={() => setIsPanelOpen(false)}
        />
      )}
    </div>
  );
}
