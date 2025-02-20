import { useState } from 'react';
import SimpleHeading from '@/components/Heading-Simple';
import { Card, CardContent } from '@/components/ui/card';
import * as z from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { toast } from '@/components/ui/use-toast';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { createIndex } from '@/services/indexes-service';
import { CreateIndexDto } from '@/models/indexmetadata';

const formSchema = z.object({
  name: z
    .string()
    .min(1, { message: 'Name is required' })
    .regex(/^[a-z][a-z0-9-]*$/, {
      message: 'Name must be lowercase, start with a letter and contain only letters, numbers and hyphens'
    }),
  description: z.string(),
  chunkSize: z.number().min(2000).max(5000).default(2300),
  chunkOverlap: z.number().min(25).max(50).default(25),
  group: z.string().optional()
});
type FormValues = z.infer<typeof formSchema>;

export default function IndexForm() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const fileId = searchParams.get('id');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      name: '',
      description: '',
      chunkSize: 2300,
      chunkOverlap: 25,
      group: undefined
    }
  });

  const onSubmit = async (values: FormValues) => {
    setIsSubmitting(true);
    try {
      // Construct the fileData object without id and createdAt
      const indexData = values as CreateIndexDto;
      const createdIndex = await createIndex(indexData);
      if (createdIndex) {
        toast({
          title: 'Success',
          description: 'Index created successfully'
        });
        navigate('/containers');
      } else {
        toast({
          variant: 'destructive',
          title: 'Error',
          description: 'Operation failed. Please try again or check logs for details.'
        });
      }
    } catch (error) {
      toast({
        variant: 'destructive',
        title: 'Error',
        description: error instanceof Error ? error.message : 'An unexpected error occurred'
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="flex h-screen bg-background text-foreground">
      <div className="flex-1 flex flex-col">
        <SimpleHeading Title="Containers" Subtitle={'Create New Container'} DocumentCount={0} />
        <div className="flex flex-col h-full grow min-h-0 overflow-auto">
          <Card>
            <CardContent className="space-y-8 mt-8">
              <Form {...form}>
                <FormField
                  control={form.control}
                  name="name"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Name</FormLabel>
                      <FormControl>
                        <Input {...field} placeholder="Your container name" />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

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
                  name="chunkSize"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Chunk Size</FormLabel>
                      <FormControl>
                        <Input
                          {...field}
                          onChange={(val) => field.onChange(+val.target.value)}
                          type="number"
                          min={2000}
                          max={5000}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="chunkOverlap"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Chunk Overlap (%)</FormLabel>
                      <FormControl>
                        <Input
                          {...field}
                          onChange={(val) => field.onChange(+val.target.value)}
                          type="number"
                          min={25}
                          max={50}
                        />
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
                        <Input {...field} placeholder="" />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <div className="flex justify-between mt-2">
                  <Button type="submit" disabled={isSubmitting} onClick={form.handleSubmit(onSubmit)}>
                    {isSubmitting ? 'Submitting...' : fileId ? 'Update' : 'Create'}
                  </Button>
                </div>
              </Form>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
