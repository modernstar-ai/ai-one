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
//import { MultiSelectInput } from '@/components/ui-extended/multi-select';
//import { useFolders } from '@/hooks/use-folders';

//import { MultiToolSettingsDropdownInput } from '@/components/MultiToolSelector';
import { fetchTools } from '@/services/toolservice';
//import { Tool } from '@/types/Tool';
import { useIndexes } from '@/hooks/use-indexes';
import { Loader2 } from 'lucide-react';
import { Checkbox } from '@/components/ui/checkbox';
import {
  PermissionsAccessControl,
  permissionsAccessControlDefaultValues
} from '@/components/ui-extended/permissions-access-control';
import { PermissionsAccessControlSchema } from '@/components/ui-extended/permissions-access-control/form';
import { MultiInput } from '@/components/ui-extended/multi-input';
//import { enablePreviewFeatures } from '@/globals';

// Define the AssistantPromptOptions schema
const AssistantPromptOptionsSchema = z.object({
  systemPrompt: z.string(),
  temperature: z.number(),
  topP: z.number().min(0).max(1).optional(),
  maxTokens: z.number().int().max(16384).optional()
});

// Define the AssistantFilterOptions schema
const AssistantFilterOptionsSchema = z.object({
  indexName: z.string(),
  limitKnowledgeToIndex: z.boolean(),
  allowInThreadFileUploads: z.boolean(),
  documentLimit: z.number().int(),
  strictness: z.number().min(1).max(5).optional(),
  folders: z.array(z.string()),
  tags: z.array(z.string()).refine((arr) => arr.every((tag) => tag.trim().length > 0), {
    message: 'Each tag must contain at least one character'
  })
});

// Define the main schema
const formSchema = z.object({
  name: z.string().min(1, { message: 'Name is required' }),
  description: z.string(),
  greeting: z.string(),
  type: z.nativeEnum(AssistantType),
  status: z.nativeEnum(AssistantStatus),
  promptOptions: AssistantPromptOptionsSchema,
  filterOptions: AssistantFilterOptionsSchema,
  accessControl: PermissionsAccessControlSchema
});

type FormValues = z.infer<typeof formSchema>;

export default function AssistantForm() {
  const [loading, setLoading] = useState<boolean>(true);
  const [searchParams] = useSearchParams();
  const fileId = searchParams.get('id');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const { toast } = useToast();
  const navigate = useNavigate();
  //const { folders } = useFolders();
  const { indexes } = useIndexes();
  //const [selectedToolIds, setSelectedToolIds] = useState<Set<string>>(new Set());
  //const [tools, setTools] = useState<Tool[]>([]);

  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      name: '',
      description: '',
      greeting: '',
      type: AssistantType.Chat,
      status: AssistantStatus.Draft,
      promptOptions: {
        systemPrompt: '',
        temperature: 0.7,
        topP: 0.95,
        maxTokens: undefined
      },
      filterOptions: {
        indexName: '',
        limitKnowledgeToIndex: false,
        allowInThreadFileUploads: false,
        documentLimit: 5,
        strictness: undefined,
        folders: [],
        tags: []
      },
      accessControl: permissionsAccessControlDefaultValues
    }
  });

  const getTools = async () => {
    try {
      const response = await fetchTools();
      if (response) {
        //setTools(response.sort((a, b) => a.name.localeCompare(b.name)));
      }
    } catch (error) {
      console.error('Failed to fetch tools:', error);
    }
  };

  const loadAssistant = async () => {
    if (fileId) {
      toast({ title: 'load', description: 'Loading...' });
      const file = (await fetchAssistantById(fileId)) as Assistant;
      if (file) {
        form.reset({
          name: file.name,
          description: file.description,
          greeting: file.greeting,
          type: file.type,
          status: file.status,
          promptOptions: {
            systemPrompt: file.promptOptions.systemPrompt,
            temperature: file.promptOptions.temperature,
            topP: file.promptOptions.topP,
            maxTokens: file.promptOptions.maxTokens ?? undefined
          },
          filterOptions: {
            indexName: file.filterOptions.indexName,
            limitKnowledgeToIndex: file.filterOptions.limitKnowledgeToIndex,
            allowInThreadFileUploads: file.filterOptions.allowInThreadFileUploads,
            documentLimit: file.filterOptions.documentLimit,
            strictness: file.filterOptions.strictness ?? undefined,
            folders: file.filterOptions.folders ?? [],
            tags: file.filterOptions.tags ?? []
          },
          accessControl: file.accessControl ?? permissionsAccessControlDefaultValues
        });
      } else {
        toast({
          variant: 'destructive',
          title: 'Error',
          description: 'Failed to load assistant data'
        });
      }
    }
  };

  useEffect(() => {
    const load = async () => {
      setLoading(true);
      await getTools();
      await loadAssistant();
      setLoading(false);
    };

    load();
  }, []);

  // useEffect(() => {
  //   form.setValue(
  //     'tools',
  //     Array.from(selectedToolIds).map((toolId) => {
  //       const tool = tools.find((t) => t.id === toolId);
  //       return tool ? { toolId: tool.id, toolName: tool.name } : { toolId: '', toolName: '' };
  //     })
  //   );
  // }, [selectedToolIds, tools, form]);

  const onSubmit = async (values: FormValues) => {
    setIsSubmitting(true);
    try {
      const fileData = values as Assistant;

      if (fileId) await updateAssistant(fileData, fileId);
      else await createAssistant(fileData);

      toast({
        title: 'Success',
        description: fileId ? 'Assistant updated successfully' : 'Assistant created successfully'
      });
      navigate('/assistants');
    } catch (error) {
      toast({
        variant: 'destructive',
        title: 'Error',
        description: error instanceof Error ? error.message : 'An error occurred'
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  if (loading) {
    return (
      <div className="flex flex-col h-full w-full justify-center items-center bg-background text-foreground">
        <Loader2 className="h-32 w-32 animate-spin" />
        <div className="font-medium">Loading Assistant...</div>
      </div>
    );
  }

  return (
    <div className="flex h-screen bg-background text-foreground">
      {/* Main Content Area */}
      <div className="flex-1 flex flex-col">
        {/* Header */}
        <SimpleHeading
          Title="AI Assistants"
          Subtitle={fileId ? 'Edit AI Assistant' : 'Create New AI Assistant'}
          DocumentCount={0}
        />

        <div className="flex flex-col h-full grow min-h-0 overflow-auto">
          <Card>
            <CardContent className="space-y-8">
              <Form {...form}>
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
                      <FormLabel htmlFor="type-select">Type</FormLabel>
                      <Select onValueChange={(value) => field.onChange(value as AssistantType)} value={field.value}>
                        <FormControl>
                          <SelectTrigger id="type-select" aria-labelledby="type-select-label">
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
                  name="promptOptions.systemPrompt"
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
                  name="accessControl"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Security Access</FormLabel>
                      <FormControl>
                        <div>
                          <PermissionsAccessControl onChange={field.onChange} pac={field.value} />
                        </div>
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="filterOptions.indexName"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel htmlFor="container-select">Container</FormLabel>
                      <FormControl>
                        <Select onValueChange={(value) => field.onChange(value)} value={field.value}>
                          <FormControl>
                            <SelectTrigger id="container-select" aria-labelledby="container-label">
                              <SelectValue placeholder="Select Container" />
                            </SelectTrigger>
                          </FormControl>

                          <SelectContent>
                            {indexes?.map((index) => (
                              <SelectItem key={index.id} value={index.name}>
                                {index.name}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="filterOptions.limitKnowledgeToIndex"
                  render={({ field }) => (
                    <FormItem className="flex items-center space-y-0">
                      <FormLabel htmlFor="container-select" className="my-auto">
                        Limit Assistant knowledge to container only
                      </FormLabel>
                      <FormControl>
                        <Checkbox className="p-3 ms-2" checked={field.value} onCheckedChange={field.onChange} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="filterOptions.allowInThreadFileUploads"
                  render={({ field }) => (
                    <FormItem className="flex items-center space-y-0">
                      <FormLabel htmlFor="container-select" className="my-auto">
                        Allow in thread file uploads
                      </FormLabel>
                      <FormControl>
                        <Checkbox className="p-3 ms-2" checked={field.value} onCheckedChange={field.onChange} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="filterOptions.folders"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Folder Filters</FormLabel>
                      <FormControl>
                        <MultiInput {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="filterOptions.tags"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Tag Filters</FormLabel>
                      <FormControl>
                        <MultiInput {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="filterOptions.documentLimit"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Document Limit</FormLabel>
                      <FormControl>
                        <Input
                          {...field}
                          type="number"
                          placeholder="Enter a number between 0 and 1000"
                          min={0}
                          max={1000}
                          onChange={(e) => field.onChange(Number(e.target.value))}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                {/* {enablePreviewFeatures == true && (
                  <FormField
                    control={form.control}
                    name="tools"
                    render={() => (
                      <FormItem>
                        <FormLabel>Add Functionalities to the Bots</FormLabel>
                        <MultiToolSettingsDropdownInput
                          tools={tools}
                          selectedToolIds={selectedToolIds}
                          setSelectedToolIds={setSelectedToolIds}
                        />
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                )} */}

                <FormField
                  control={form.control}
                  name="promptOptions.temperature"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel id="temperature-label">Temperature</FormLabel>
                      <FormControl>
                        <Slider
                          value={[field.value]}
                          min={0}
                          max={2}
                          step={0.1}
                          onValueChange={(value) => {
                            field.onChange(value[0]);
                          }}
                          aria-label="Temperature slider"
                        />
                      </FormControl>
                      <FormMessage />
                      <div id="temperature-value">Selected Temperature: {field.value}</div>
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="promptOptions.topP"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel id="topP-label">Top P</FormLabel>
                      <FormControl>
                        <Slider
                          value={[field.value ?? 1]}
                          min={0}
                          max={1}
                          step={0.01}
                          onValueChange={(value) => field.onChange(value[0])}
                          aria-label="topP slider"
                        />
                      </FormControl>
                      <FormMessage />
                      <div id="topP-value">Selected Top P: {field.value}</div>
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="promptOptions.maxTokens"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Max Response (tokens)</FormLabel>
                      <FormControl>
                        <Input
                          {...field}
                          type="number"
                          placeholder="Enter a number, by default value will be set to 800 tokens"
                          onChange={(e) => field.onChange(Number(e.target.value))}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                {/* {enablePreviewFeatures == true && (
                  <FormField
                    control={form.control}
                    name="pastMessages"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Past Messages Included</FormLabel>
                        <FormControl>
                          <Input
                            {...field}
                            type="number"
                            placeholder="Enter a number, by default value will be set to 10"
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                )} */}
                <FormField
                  control={form.control}
                  name="filterOptions.strictness"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel id="strictness-label">Strictness</FormLabel>
                      <FormControl>
                        <Slider
                          value={[field.value ?? 0]}
                          min={1}
                          max={5}
                          step={1}
                          onValueChange={(value) => {
                            field.onChange(value[0]);
                          }}
                          aria-label="strictness slider"
                        />
                      </FormControl>
                      <FormMessage />
                      <div id="strictness-value">Strictness: {field.value}</div>
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="status"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Status</FormLabel>
                      <Select onValueChange={(value) => field.onChange(value as AssistantStatus)} value={field.value}>
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

                <div className="flex justify-between mt-2">
                  <Button
                    type="submit"
                    disabled={isSubmitting}
                    onClick={form.handleSubmit(onSubmit)}
                    aria-label="Save Assistant">
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
