import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form';
import { Input } from '@/components/ui/input';
import { Index } from '@/models/indexmetadata';
import { updateIndex } from '@/services/indexes-service';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import * as z from 'zod';

const editFormSchema = z.object({
  id: z.string(),
  description: z.string(),
  group: z.string(),
});
type FormValues = z.infer<typeof editFormSchema>;

interface IEditIndexProps {
  index: Index | undefined;
  setIndexEditing: (index: Index | undefined) => void;
  refreshIndexes: () => Promise<void>;
}
export const EditIndexDialog = (props: IEditIndexProps) => {
  const { index, setIndexEditing, refreshIndexes } = props;

  const form = useForm<FormValues>({
    resolver: zodResolver(editFormSchema),
    values: {
      id: index?.id ?? '',
      description: index?.description ?? '',
      group: index?.group ?? '',
    },
  });

  const onSubmit = async (values: FormValues) => {
    try {
      await updateIndex(values);
      await refreshIndexes();
      setIndexEditing(undefined);
    } catch (err) {
      console.log(err);
    }
  };

  return (
    <Dialog open={index !== undefined} onOpenChange={(isOpen) => !isOpen && setIndexEditing(undefined)}>
      <DialogContent className="sm:max-w-[425px]">
        {index && (
          <Form {...form}>
            <DialogHeader>
              <DialogTitle>Edit Container</DialogTitle>
              <DialogDescription>Make changes to your container here. Click save when you're done.</DialogDescription>
            </DialogHeader>
            <FormField
              control={form.control}
              name="description"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Description</FormLabel>
                  <FormControl>
                    <Input {...field} placeholder="A brief overview of your container" />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="group"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Group</FormLabel>
                  <FormControl>
                    <Input {...field} placeholder="Security group" />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <DialogFooter>
              <Button type="submit" disabled={form.formState.isSubmitting} onClick={form.handleSubmit(onSubmit)}>
                Save changes
              </Button>
            </DialogFooter>
          </Form>
        )}
      </DialogContent>
    </Dialog>
  );
};
