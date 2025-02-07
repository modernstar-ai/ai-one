import React, { useState } from 'react';
import { Settings2 } from 'lucide-react';
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetTrigger } from '@/components/ui/sheet';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Label } from '@/components/ui/label';
import { fetchChatThread, updateChatThread } from '@/services/chatthreadservice';
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
import { FoldersFilterInput } from './ui-extended/folder-filter';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from './ui/form';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { ChatThread } from '@/types/ChatThread';
import { toast } from './ui/use-toast';

interface SimpleHeadingProps {
  Title: string;
  Subtitle: string;
  DocumentCount: number;
  threadId?: string;
}

const formSchema = z.object({
  folders: z.array(z.string()),
});

const SimpleHeading: React.FC<SimpleHeadingProps> = ({
  Title,
  Subtitle,
  //DocumentCount,
  threadId,
}) => {
  const [thread, setThread] = useState<ChatThread | null>(null);
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

  type FormValues = z.infer<typeof formSchema>;
  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      folders: thread?.filterOptions.folders ?? [],
    },
  });

  const onSave = async (form: FormValues) => {
    if (thread) {
      const newThread: ChatThread = { ...thread };
      newThread.filterOptions.folders = form.folders ?? [];
      await updateChatThread(newThread);
      toast({
        title: 'Saved configuration',
      });
    }
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

        setThread(thread);
        form.setValue('folders', thread.filterOptions.folders ?? []);
      } catch (err) {
        console.error('Error fetching thread:', err);
        setError(err instanceof Error ? err.message : 'An error occurred while loading configuration');
        setThread(null);
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

    if (!thread) {
      return <div className="text-center py-4 text-sm text-muted-foreground">No configuration available</div>;
    }

    return (
      <div className="flex flex-col h-full">
        {Object.entries({
          temperature: thread.promptOptions.temperature,
          topP: thread.promptOptions.topP,
          maxResponseToken: thread.promptOptions.maxTokens,
          strictness: thread.filterOptions.strictness,
          documentLimit: thread.filterOptions.documentLimit,
        }).map(([key, value]) => (
          <Card key={key} className="border-none">
            <CardContent className="p-3">
              <Label className="text-sm font-medium text-muted-foreground">{getConfigLabel(key)}</Label>
              <div className="mt-1 text-sm">{formatValue(value)}</div>
            </CardContent>
          </Card>
        ))}

        <Form {...form}>
          <div className="overflow-auto pb-2">
            <FormField
              control={form.control}
              name="folders"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Folder Filters</FormLabel>
                  <FormControl>
                    <FoldersFilterInput {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
          </div>

          <div className="mt-auto justify-between">
            <Button
              type="submit"
              disabled={form.formState.isSubmitting}
              onClick={form.handleSubmit(onSave)}
              aria-label="Save Assistant"
            >
              {form.formState.isSubmitting ? 'Saving...' : 'Save'}
            </Button>
          </div>
        </Form>
      </div>
    );
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
              <Button variant="outline" size="icon" className="h-8 w-8" aria-label="Open settings">
                <Settings2 className="h-4 w-4" />
              </Button>
            </SheetTrigger>
            <SheetContent className="flex flex-col h-full">
              <SheetHeader>
                <SheetTitle>Chat Configuration</SheetTitle>
              </SheetHeader>
              <div className="overflow-auto grow">{renderContent()}</div>
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
