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
import { cn } from '@/lib/utils';
import { Loader2, Plus, Trash2 } from 'lucide-react';

interface Thread {
  id: string;
  name: string;
}

interface Props {
  threads?: Thread[];
  loading: boolean;
  error?: string;
  handleCreateChat: () => void;
  handleDeleteThread: (threadId: string) => void;
  handleClearHistory: () => void;
  navigate: (path: string) => void;
  setIsMobileMenuOpen: (open: boolean) => void;
}

export const ChatThreads = ({
  threads,
  loading,
  error,
  handleCreateChat,
  handleDeleteThread,
  handleClearHistory,
  navigate,
  setIsMobileMenuOpen
}: Props) => (
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
