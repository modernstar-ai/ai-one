'use client';

import { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import SimpleHeading from '@/components/Heading-Simple';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Settings } from 'lucide-react';
import { Slider } from '@/components/ui/slider';
import { Card, CardContent } from '@/components/ui/card';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useNavigate } from 'react-router-dom';
import * as z from 'zod';
import { useToast } from '@/components/ui/use-toast';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form';

import { createAssistant, fetchAssistantById, updateAssistant } from '@/services/assistantservice';
import { Assistant, AssistantStatus, AssistantType, InsertAssistant } from '@/types/Assistant';
import { fetchTools } from '@/services/toolservice';
import { useIndexes } from '@/hooks/use-indexes';
import { Loader2 } from 'lucide-react';
import { Checkbox } from '@/components/ui/checkbox';
import {
  PermissionsAccessControl,
  permissionsAccessControlDefaultValues
} from '@/components/ui-extended/permissions-access-control';
import { PermissionsAccessControlSchema } from '@/components/ui-extended/permissions-access-control/form';
import { MultiInput } from '@/components/ui-extended/multi-input';
import { MODEL_CONFIG_DEFAULTS } from '@/configs/form-default-values/assistant';
import { BaseDialog } from '@/components/base/BaseDiaglog';
import useGetTextModels from '@/hooks/use-get-textmodels';

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

// Enhanced model options schema with proper validation
const ModelOptionsSchema = z
  .object({
    allowModelSelection: z.boolean(),
    models: z.array(
      z.object({
        modelId: z.string(),
        isSelected: z.boolean()
      })
    ),
    defaultModelId: z.string()
  })
  .refine(
    (data) => {
      // If model selection is enabled, at least one model must be selected
      if (data.allowModelSelection) {
        return data.models.some((model) => model.isSelected);
      }
      return true;
    },
    {
      message: 'At least one model must be selected when model selection is enabled',
      path: ['models']
    }
  );

// Define the main schema
const formSchema = z.object({
  name: z.string().min(1, { message: 'Name is required' }),
  description: z.string(),
  greeting: z.string(),
  type: z.nativeEnum(AssistantType),
  status: z.nativeEnum(AssistantStatus),
  promptOptions: AssistantPromptOptionsSchema,
  filterOptions: AssistantFilterOptionsSchema,
  accessControl: PermissionsAccessControlSchema,
  modelOptions: ModelOptionsSchema
});

type FormValues = z.infer<typeof formSchema>;

export default function AssistantForm() {
  const [loading, setLoading] = useState<boolean>(true);
  const [searchParams] = useSearchParams();
  const fileId = searchParams.get('id');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const { toast } = useToast();
  const navigate = useNavigate();
  const { indexes } = useIndexes();

  const { data, isLoading: textmodelsLoading } = useGetTextModels();

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
      accessControl: permissionsAccessControlDefaultValues,
      modelOptions: MODEL_CONFIG_DEFAULTS
    }
  });

  const { watch } = form;
  const watchAllFields = watch();

  const getTools = async () => {
    try {
      const response = await fetchTools();
      if (response) {
        // Handle tools if needed
      }
    } catch (error) {
      console.error('Failed to fetch tools:', error);
    }
  };

  const loadAssistant = async () => {
    if (fileId) {
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
          accessControl: file.accessControl ?? permissionsAccessControlDefaultValues,
          modelOptions: file.modelOptions ?? MODEL_CONFIG_DEFAULTS
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

  // Handle model selection changes
  // Handle model selection changes
  const handleModelSelectionChange = (modelIndex: number, isSelected: boolean) => {
    const currentModels = watchAllFields.modelOptions.models;
    const updatedModels = [...currentModels];
    updatedModels[modelIndex] = { ...updatedModels[modelIndex], isSelected };

    form.setValue('modelOptions.models', updatedModels);

    const currentDefault = watchAllFields.modelOptions.defaultModelId;
    const changedModel = updatedModels[modelIndex];

    if (isSelected) {
      // If checking a model and no default is set, set this as default
      if (!currentDefault || !updatedModels.some((m) => m.isSelected && m.modelId === currentDefault)) {
        form.setValue('modelOptions.defaultModelId', changedModel.modelId);
      }
    } else {
      // If unchecking a model and it's the current default, find a new default
      if (changedModel.modelId === currentDefault) {
        const firstSelectedModel = updatedModels.find((m) => m.isSelected);
        form.setValue('modelOptions.defaultModelId', firstSelectedModel?.modelId || '');
      }
    }

    // Trigger validation with a slight delay to ensure state is updated
    setTimeout(() => {
      form.trigger('modelOptions');
    }, 0);
  };
  const onSubmit = async (values: FormValues) => {
    setIsSubmitting(true);
    try {
      const fileData = values as InsertAssistant;

      if (fileId) {
        await updateAssistant(fileData, fileId);
      } else {
        await createAssistant(fileData);
      }

      toast({
        title: 'Success',
        description: fileId ? 'Assistant updated successfully' : 'Assistant created successfully'
      });
      navigate('/assistants');
    } catch (error: any) {
      // Enhanced error handling for 400 responses
      let errorMessage = 'An error occurred';

      if (error?.response?.status === 400) {
        // Handle validation errors from the API
        if (error.response.data?.errors) {
          // If the API returns structured validation errors
          const validationErrors = Object.values(error.response.data.errors).flat();
          errorMessage = validationErrors.join(', ');
        } else if (error.response.data?.message) {
          errorMessage = error.response.data.message;
        } else {
          errorMessage = 'Validation failed. Please check your input.';
        }
      } else if (error instanceof Error) {
        errorMessage = error.message;
      }

      toast({
        variant: 'destructive',
        title: 'Error',
        description: errorMessage
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
    <div className="flex min-h-screen lg:h-screen lg:overflow-y-auto bg-background text-foreground">
      {/* Main Content Area */}
      <div className="flex-1 flex flex-col">
        {/* Header */}
        <SimpleHeading
          Title="AI Assistants"
          Subtitle={fileId ? 'Edit AI Assistant' : 'Create New AI Assistant'}
          DocumentCount={0}
        />

        <div className="flex flex-col h-full grow min-h-0 lg:pt-0 pt-4 overflow-auto">
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
                {textmodelsLoading ? (
                  <div className="flex justify-center items-center">
                    <Loader2 className="animate-spin" />
                  </div>
                ) : (
                  <>
                    <FormField
                      control={form.control}
                      name="modelOptions.allowModelSelection"
                      render={({ field }) => (
                        <FormItem className="flex items-center space-y-0">
                          <FormLabel htmlFor="allow-model-selection" className="my-auto mr-3">
                            Allow Model Selection
                          </FormLabel>
                          <FormControl>
                            <Checkbox
                              id="allow-model-selection"
                              checked={field.value}
                              onCheckedChange={field.onChange}
                            />
                          </FormControl>
                        </FormItem>
                      )}
                    />

                    {watchAllFields.modelOptions.allowModelSelection ? (
                      <>
                        <p className="text-red-500">{form.formState.errors.modelOptions?.defaultModelId?.message}</p>
                        <p className="text-red-500">{form.formState.errors.modelOptions?.models?.message}</p>
                      </>
                    ) : null}
                    {/* Model Configuration Dialog */}
                    <BaseDialog
                      title="Model Configuration"
                      description="Manage the models available for the assistant."
                      label={
                        <div className="flex justify-center items-center space-x-2">
                          <span className="mr-4">
                            <Settings />
                          </span>
                          Model Configuration
                        </div>
                      }
                      disabled={!watchAllFields.modelOptions.allowModelSelection}>
                      <div className="space-y-4">
                        <FormLabel>Available Models</FormLabel>
                        {data?.models?.map((model, index) => (
                          <div key={model.modelId} className="flex items-center justify-between p-4 border rounded-lg">
                            <div className="flex items-center space-x-4">
                              <Checkbox
                                checked={form.getValues(`modelOptions.models.${index}.isSelected`) || false}
                                onCheckedChange={(checked) => handleModelSelectionChange(index, checked as boolean)}
                              />
                              <div className="flex-1">
                                <div className="font-medium">{model.modelId}</div>
                              </div>
                            </div>
                          </div>
                        ))}

                        {/* Display validation error for models */}
                        <FormField
                          control={form.control}
                          name="modelOptions.models"
                          render={() => (
                            <FormItem>
                              <FormMessage />
                            </FormItem>
                          )}
                        />
                      </div>

                      <FormField
                        control={form.control}
                        name="modelOptions.defaultModelId"
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel htmlFor="default-model-select">Default Model</FormLabel>
                            <Select onValueChange={field.onChange} value={field.value}>
                              <FormControl>
                                <SelectTrigger id="default-model-select">
                                  <SelectValue placeholder="Select Default Model" />
                                </SelectTrigger>
                              </FormControl>
                              <SelectContent>
                                {form
                                  .getValues('modelOptions.models')
                                  ?.filter((m) => m.isSelected)
                                  .map((model) => (
                                    <SelectItem key={model.modelId} value={model.modelId}>
                                      {model.modelId}
                                    </SelectItem>
                                  ))}
                              </SelectContent>
                            </Select>
                          </FormItem>
                        )}
                      />
                    </BaseDialog>
                  </>
                )}
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
