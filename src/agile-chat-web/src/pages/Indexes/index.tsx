import { useState } from "react";
import SimpleHeading from "@/components/Heading-Simple";
import { Card, CardContent } from "@/components/ui/card";
import * as z from 'zod';
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form';
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Indexes } from "@/models/indexmetadata";
import { toast } from "@/components/ui/use-toast";
import { useNavigate, useSearchParams } from "react-router-dom";
import { createIndex } from "@/services/indexes-service";

const formSchema = z.object({
  name: z.string().min(1, { message: 'Name is required' }),
  description: z.string(),
  group: z.string(),
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
      group: '',
    },
  });

  const onSubmit = async (values: FormValues) => {
    setIsSubmitting(true);
    try {
      // Construct the fileData object without id and createdAt
      const fileData: Partial<Indexes> = {
        ...values,
        description: values.description ?? '',
        group: values.group ?? '', // Ensure this uses the correct value from `values`
        createdBy: 'adam@stephensen.me',
      };
      console.log("Sending fileData:", fileData);

      // Call the createIndex function, passing only the required fields
      const result = await createIndex(fileData);
  
      if (result) {
        toast({
          title: 'Success',
          description: 'Index created successfully',
        });
        navigate('/indexes');
      } else {
        throw new Error('Operation failed');
      }
    } catch (error) {
      toast({
        variant: 'destructive',
        title: 'Error',
        description: error instanceof Error ? error.message : 'An error occurred',
      });
    } finally {
      setIsSubmitting(false);
    }
  };
  

  return (
    <div className="flex h-screen bg-background text-foreground">
      <div className="flex-1 flex flex-col">
        <SimpleHeading
            Title="Containers"
            Subtitle={'Create New Container Index'}
            DocumentCount={0}
          />
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
                            <Input {...field} placeholder="Your Indexer Name" />
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
                            <Input {...field} placeholder="A brief overview of your Indexer" />
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
                        <FormLabel>Security Group</FormLabel>
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
  )
}