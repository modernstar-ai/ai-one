import { MultiInput } from '@/components/ui-extended/multi-input';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger
} from '@/components/ui/dialog';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form';
import { toast } from '@/components/ui/use-toast';
import { CosmosFile } from '@/models/filemetadata';
import { updateFile } from '@/services/files-service';
import { zodResolver } from '@hookform/resolvers/zod';
import { Pencil } from 'lucide-react';
import React, { useRef } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';

interface FileViewingDialogProps {
  file: CosmosFile;
  customTrigger?: React.ReactNode;
  handleRefresh: () => Promise<void>;
}
export function FileEditDialog(props: FileViewingDialogProps) {
  const { file, handleRefresh, customTrigger } = props;
  const closeRef = useRef<HTMLButtonElement>(null);

  const formSchema = z.object({
    tags: z.array(z.string()).refine((arr) => arr.every((tag) => tag.trim().length > 0), {
      message: 'Each tag must contain at least one character'
    })
  });
  type FormValues = z.infer<typeof formSchema>;

  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      tags: file.tags ?? []
    }
  });

  const onSubmit = async (values: FormValues) => {
    await updateFile(file.id, values.tags)
      .then(() => {
        handleRefresh();
        closeRef.current?.click();
      })
      .catch((err) =>
        toast({
          title: 'Error',
          description: `Failed request with error: ${err}`,
          variant: 'destructive'
        })
      );
  };

  return (
    <Dialog>
      <DialogTrigger asChild>
        {customTrigger ? (
          customTrigger
        ) : (
          <Button variant={'outline'} size={'icon'} className="w-8 h-8" title={`Edit ${file.name}`}>
            <Pencil />
          </Button>
        )}
      </DialogTrigger>
      <DialogContent className="flex flex-col">
        <DialogHeader>
          <DialogTitle>Edit {file.name}</DialogTitle>
          <DialogDescription>
            Editing a file triggers a re-indexing process which may take some time to update
          </DialogDescription>
        </DialogHeader>
        <Form {...form}>
          <div className="flex flex-col grow min-h-0 w-full h-full justify-center">
            <FormField
              control={form.control}
              name="tags"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Tags</FormLabel>
                  <FormControl>
                    <MultiInput onBlur={field.onBlur} onChange={field.onChange} value={field.value} ref={field.ref} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
          </div>
          <DialogFooter>
            <DialogClose ref={closeRef} asChild>
              <Button type="button">Close</Button>
            </DialogClose>
            <Button
              variant="secondary"
              type="submit"
              onClick={form.handleSubmit(onSubmit)}
              disabled={!form.formState.isValid || form.formState.isSubmitting}>
              Save
            </Button>
          </DialogFooter>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
