import React, { useState } from 'react';
import { Settings2 } from 'lucide-react';
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetTrigger } from '@/components/ui/sheet';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Label } from '@/components/ui/label';
import { fetchChatThread } from '@/services/chatthreadservice';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import { useLocation } from 'react-router-dom';

interface ThreadConfig {
  temperature?: number | null;
  topP?: number | null;
  maxResponseToken?: number | null;
  strictness?: number | null;
  documentLimit?: number;
}

interface SimpleHeadingProps {
  Title: string;
  Subtitle: string;
  DocumentCount: number;
  threadId?: string;
}

const SimpleHeading: React.FC<SimpleHeadingProps> = ({
  Title,
  Subtitle,
  //DocumentCount,
  threadId,
}) => {
  const [threadConfig, setThreadConfig] = useState<ThreadConfig | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const location = useLocation();

  const isChatPage = location.pathname.startsWith('/chat/');

  const formatValue = (value: number | string | null | undefined): string => {
    if (value === null || value === undefined || value == 0) {
      return 'Not set';
    }

    if (typeof value === 'number') {
      return value.toString();
    }

    const trimmedValue = value.trim();
    return trimmedValue || 'Not set';
  };

  const handleSheetOpen = async (open: boolean) => {
    if (open && threadId) {
      setIsLoading(true);
      setError(null);
      try {
        const thread = await fetchChatThread(threadId);
        if (!thread) {
          throw new Error('Failed to load thread configuration');
        }

        setThreadConfig({
          temperature: thread.promptOptions.temperature,
          topP: thread.promptOptions.topP,
          maxResponseToken: thread.promptOptions.maxTokens,
          strictness: thread.filterOptions.strictness,
          documentLimit: thread.filterOptions.documentLimit,
        });
      } catch (err) {
        console.error('Error fetching thread:', err);
        setError(err instanceof Error ? err.message : 'An error occurred while loading configuration');
        setThreadConfig(null);
      } finally {
        setIsLoading(false);
      }
    }
  };

  const getConfigLabel = (key: string): string => {
    return (
      key.charAt(0).toUpperCase() +
      key
        .slice(1)
        .replace(/([A-Z])/g, ' $1')
        .trim()
    );
  };

  const renderContent = () => {
    if (isLoading) {
      return <div className="text-center py-4 text-sm text-muted-foreground">Loading configuration...</div>;
    }

    if (!threadConfig) {
      return <div className="text-center py-4 text-sm text-muted-foreground">No configuration available</div>;
    }

    return Object.entries(threadConfig).map(([key, value]) => (
      <Card key={key} className="border-none">
        <CardContent className="p-3">
          <Label className="text-sm font-medium text-muted-foreground">{getConfigLabel(key)}</Label>
          <div className="mt-1 text-sm">{formatValue(value)}</div>
        </CardContent>
      </Card>
    ));
  };

  return (
    <>
      <div className="bg-muted p-4 flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold">{Title || 'Untitled'}</h1>
          <p className="text-sm text-muted-foreground">{Subtitle || 'No subtitle'}</p>
        </div>
        {isChatPage && threadId && (
          <Sheet onOpenChange={handleSheetOpen}>
            <SheetTrigger asChild>
              <Button variant="outline" size="icon" className="h-8 w-8">
                <Settings2 className="h-4 w-4" />
              </Button>
            </SheetTrigger>
            <SheetContent>
              <SheetHeader>
                <SheetTitle>Chat Configuration</SheetTitle>
              </SheetHeader>
              <div className="mt-6 space-y-4">{renderContent()}</div>
            </SheetContent>
          </Sheet>
        )}
      </div>

      <AlertDialog open={!!error} onOpenChange={() => setError(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Error</AlertDialogTitle>
            <AlertDialogDescription>{error}</AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogAction onClick={() => setError(null)}>OK</AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </>
  );
};

export default SimpleHeading;
