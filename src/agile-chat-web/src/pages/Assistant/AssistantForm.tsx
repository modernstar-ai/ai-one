'use client';

import { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import SimpleHeading from '@/components/Heading-Simple';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Slider } from '@/components/ui/slider';

import { Card, CardContent } from '@/components/ui/card';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useNavigate } from 'react-router-dom';
import * as z from 'zod';
import { useToast } from '@/components/ui/use-toast';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form';

import { createAssistant, fetchAssistantById, updateAssistant } from '@/services/assistantservice';
import { Assistant, AssistantStatus, AssistantType } from '@/types/Assistant';
import { MultiSelectInput } from '@/components/ui-extended/multi-select';
import { useFolders } from '@/hooks/use-folders';

//todo: replace with server side implementation
const generateGuid = () => {
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
    const r = (Math.random() * 16) | 0;
    const v = c === 'x' ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
};

// Define schema with Zod
const formSchema = z.object({
  name: z.string().min(1, { message: 'Name is required' }),
  description: z.string(),
  type: z.nativeEnum(AssistantType),
  greeting: z.string(),
  systemMessage: z.string(),
  group: z.string().optional(),
  folder: z.array(z.string()),
  temperature: z.number(),
  documentLimit: z.number(),
  status: z.nativeEnum(AssistantStatus),
});

type FormValues = z.infer<typeof formSchema>;

export default function AssistantForm() {
  const [searchParams] = useSearchParams();
  const fileId = searchParams.get('id');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [fileDates, setFileDates] = useState({
    createdAt: '', // Changed to lowercase
    updatedAt: '', // Changed to lowercase
  });
  const { toast } = useToast();
  const navigate = useNavigate();
  const { folders } = useFolders();

  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      name: '',
      description: '',
      type: AssistantType.Chat,
      greeting: '',
      systemMessage: '',
      group: '',
      folder: [],
      temperature: 0.7,
      documentLimit: 5,
      status: AssistantStatus.Draft,
    },
  });

  // Fetch existing file if editing
  useEffect(() => {
    const loadTool = async () => {
      if (fileId) {
        toast({ title: 'load', description: 'Loading...' });
        const file = (await fetchAssistantById(fileId)) as Assistant;
        console.log('Assistant loadTool', file);
        console.log('Load file.type', file.type);
        console.log('Load file.status', file.status);

        if (file) {
          form.reset({
            name: file.name,
            description: file.description,
            type: file.type,
            greeting: file.greeting || '',
            systemMessage: file.systemMessage || '',
            group: file.group || '',
            folder: file.folder || [],
            temperature: file.temperature,
            documentLimit: file.documentLimit,
            status: file.status,
          });
          setTemperature(file.temperature);
          const statusValue1 = form.getValues('status');
          console.log('Current status value 1:', statusValue1);
          form.setValue('status', file.status);
          const statusValue2 = form.getValues('status');
          console.log('Current status value 2:', statusValue2);
          // Store dates separately
          setFileDates({
            createdAt: file.createdAt,
            updatedAt: file.updatedAt,
          });

          // Read specific form values
          const statusValue = form.getValues('status');
          const typeValue = form.getValues('type');

          console.log('Current status value:', statusValue);
          console.log('Current type value:', typeValue);
        } else {
          toast({
            variant: 'destructive',
            title: 'Error',
            description: 'Failed to load assistant data',
          });
        }
      }
    };
    loadTool();
  }, [fileId, form, toast]);

  // Form submit handler
  const onSubmit = async (values: FormValues) => {
    console.log('Assistant onSubmit', values);
    setIsSubmitting(true);
    try {
      const now = new Date().toISOString();

      const fileData: Assistant = {
        id: fileId || generateGuid(),
        name: values.name,
        description: values.description,
        type: values.type,
        greeting: values.greeting,
        systemMessage: values.systemMessage,
        group: values.group,
        folder: values.folder,
        temperature: values.temperature,
        documentLimit: values.documentLimit,
        status: values.status,
        createdAt: fileId ? fileDates.createdAt : now,
        createdBy: 'adam@stephensen.me',
        updatedAt: now,
        updatedBy: 'adam@stephensen.me',
      };
      console.log('Assistant Submit filedata', fileData);

      if (fileId) {
        const result = await updateAssistant(fileData);
        console.log('Assistant Submit update result', result);
        if (result) {
          toast({
            title: 'Success',
            description: 'Assistant updated successfully',
          });
          navigate('/assistants');
        } else {
          throw new Error('Failed to update assistant');
        }
      } else {
        console.log('Assistant Submit create');
        const result = await createAssistant(fileData);
        console.log('Assistant Submit create result', result);
        if (result) {
          toast({
            title: 'Success',
            description: 'Tool created successfully',
          });
          form.reset();
          navigate('/assistants');
        } else {
          throw new Error('Failed to create tool');
        }
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

  const [temperature, setTemperature] = useState(form.getValues('temperature'));

  return (
    <div className="flex h-screen bg-background text-foreground">
      <div className="flex-1 flex flex-col">
        <SimpleHeading
          Title="AI Assistants"
          Subtitle={fileId ? 'Edit AI Assistant' : 'Create New AI Assistant'}
          DocumentCount={0}
        />
        <div className="flex-1 p-4 overflow-auto">
          <main className="flex-1 space-y-6">
            <Card>
              <CardContent className="space-y-6">
                <Form {...form}>
                  <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-8">
                    <FormField
                      control={form.control}
                      name="name"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Name</FormLabel>
                          <FormControl>
                            <Input {...field} placeholder="Your AI Assistant" />
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
                            <Input {...field} placeholder="A brief overview of your AI Assistant" />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control}
                      name="type"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Type</FormLabel>
                          <Select onValueChange={(value) => field.onChange(value as AssistantType)} value={field.value}>
                            <FormControl>
                              <SelectTrigger>
                                <SelectValue placeholder="Select Type" />
                              </SelectTrigger>
                            </FormControl>
                            <SelectContent>
                              <SelectItem value={AssistantType.Chat}>Chat</SelectItem>
                              <SelectItem value={AssistantType.Search}>Search</SelectItem>
                            </SelectContent>
                          </Select>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control}
                      name="greeting"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Greeting</FormLabel>
                          <FormControl>
                            <Textarea {...field} placeholder={`Welcome`} className="font-mono h-[80px]" />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                    <FormField
                      control={form.control}
                      name="systemMessage"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>System Message</FormLabel>
                          <FormControl>
                            <Textarea
                              {...field}
                              placeholder={`You are a helpful AI Assistant.`}
                              className="font-mono h-[200px]"
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

                    <FormField
                      control={form.control}
                      name="folder"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Folders</FormLabel>
                          <FormControl>
                            <div>
                              <MultiSelectInput
                                className="w-full"
                                items={folders?.map((folder) => folder.name)}
                                selectedItems={field.value}
                                setSelectedItems={(values: string[]) => (field.value = values)}
                                {...field}
                              />
                            </div>
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control}
                      name="documentLimit"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Document Limit</FormLabel>
                          <FormControl>
                            <Input
                              {...field}
                              type="number"
                              id="documentLimit"
                              placeholder="Enter a number between 0 and 1000"
                              min={0}
                              max={1000}
                              onChange={(e) => field.onChange(Number(e.target.value))} // Convert to number
                            />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control}
                      name="temperature"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Temperature</FormLabel>
                          <FormControl>
                            <Slider
                              {...field}
                              value={[field.value]}
                              min={0}
                              max={2}
                              step={0.1}
                              onChange={(e) => field.onChange(Number((e.target as HTMLInputElement).value))}
                              onValueChange={(value) => {
                                field.onChange(Number(value));
                                setTemperature(value[0]);
                              }}
                            />
                          </FormControl>
                          <FormMessage />
                          <div>Selected Temperature: {temperature}</div>
                        </FormItem>
                      )}
                    />
                    <FormField
                      control={form.control}
                      name="status"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Status</FormLabel>
                          <Select
                            onValueChange={(value) => field.onChange(value as AssistantStatus)}
                            value={field.value}
                          >
                            <FormControl>
                              <SelectTrigger>
                                <SelectValue placeholder="Select status" />
                              </SelectTrigger>
                            </FormControl>
                            <SelectContent>
                              <SelectItem value={AssistantStatus.Draft}>Draft</SelectItem>
                              <SelectItem value={AssistantStatus.Published}>Published</SelectItem>
                              <SelectItem value={AssistantStatus.Archived}>Archived</SelectItem>
                            </SelectContent>
                          </Select>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <div className="flex justify-between">
                      <Button type="submit" disabled={false}>
                        {isSubmitting ? 'Submitting...' : fileId ? 'Update' : 'Create'}
                      </Button>
                    </div>
                  </form>
                </Form>
              </CardContent>
            </Card>
          </main>
        </div>
      </div>
    </div>
  );
}
