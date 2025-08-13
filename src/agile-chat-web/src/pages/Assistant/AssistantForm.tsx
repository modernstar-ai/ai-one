'use client';

import { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import SimpleHeading from '@/components/Heading-Simple';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Settings, Users, Plus, X } from 'lucide-react';
import { Slider } from '@/components/ui/slider';
import { Card, CardContent } from '@/components/ui/card';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useNavigate } from 'react-router-dom';
import * as z from 'zod';
import { useToast } from '@/components/ui/use-toast';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form';

import { createAssistant, fetchAssistantById, updateAssistant } from '@/services/assistantservice';
import {
  Assistant,
  AssistantStatus,
  AssistantType,
  SelectConnectedAgent,
  SelectConnectedAgentSchema,
  SelectAgentConfigurationSchema
} from '@/types/Assistant';
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

import { BaseDialog } from '@/components/base/BaseDiaglog';
import useGetTextModels from '@/hooks/use-get-textmodels';
import useGetAgents from '@/hooks/use-get-agents';

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
  modelOptions: ModelOptionsSchema,
  agentConfiguration: SelectAgentConfigurationSchema.optional()
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
  const { data: agents } = useGetAgents();

  // Connected agents state
  const [connectedAgents, setConnectedAgents] = useState<SelectConnectedAgent[]>([]);
  const [selectedAgentId, setSelectedAgentId] = useState<string>('');
  const [agentName, setAgentName] = useState<string>('');
  const [agentDescription, setAgentDescription] = useState<string>('');

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
      modelOptions: {
        allowModelSelection: data?.allowModelSelection || false,
        models: [],
        defaultModelId: data?.defaultModelId || 'GPT-4o'
      },
      agentConfiguration: {
        agentDescription: '',
        agentId: '',
        agentName: '',
        connectedAgents: [],
        bingConfig: {
          enableWebSearch: false,
          webResultsCount: 5
        }
      }
    }
  });

  const { watch } = form;
  const watchAllFields = watch();
  const watchType = watch('type');
  const watchEnableWebSearch = watch('agentConfiguration.bingConfig.enableWebSearch');

  //Update agents formState
  useEffect(() => {
    form.setValue('agentConfiguration.connectedAgents', connectedAgents);
  }, [connectedAgents]);

  // Function to add agent to connected agents array
  const handleAddAgent = () => {
    if (!selectedAgentId || !agentName.trim() || !agentDescription.trim()) return;

    const selectedAgent = agents?.find((agent) => agent.agentConfiguration.agentId === selectedAgentId);
    if (!selectedAgent) return;

    const newConnectedAgent: SelectConnectedAgent = {
      agentId: selectedAgentId,
      agentName: agentName.trim(),
      activationDescription: agentDescription.trim()
    };

    // Validate using Zod schema
    const validation = SelectConnectedAgentSchema.safeParse(newConnectedAgent);
    if (!validation.success) {
      toast({
        variant: 'destructive',
        title: 'Validation Error',
        description: validation.error.errors[0].message
      });
      return;
    }

    // Check if agent is already connected
    if (connectedAgents.some((agent) => agent.agentId === selectedAgentId)) {
      toast({
        variant: 'destructive',
        title: 'Error',
        description: 'This agent is already connected'
      });
      return;
    }

    setConnectedAgents((prev) => [...prev, newConnectedAgent]);
    setSelectedAgentId('');
    setAgentName('');
    setAgentDescription('');

    toast({
      title: 'Success',
      description: 'Agent added successfully'
    });
  };

  // Function to remove agent from connected agents array
  const handleRemoveAgent = (agentId: string) => {
    setConnectedAgents((prev) => prev.filter((agent) => agent.agentId !== agentId));
  };

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
          modelOptions: file.modelOptions,
          agentConfiguration: file.agentConfiguration ?? {
            agentDescription: '',
            agentId: '',
            agentName: '',
            connectedAgents: [],
            bingConfig: {
              enableWebSearch: false,
              webResultsCount: 5
            }
          }
        });

        // Populate connectedAgents state from loaded data
        if (file.agentConfiguration?.connectedAgents) {
          setConnectedAgents(file.agentConfiguration.connectedAgents);
        }
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

  // Update form when API data is loaded
  useEffect(() => {
    if (data && !textmodelsLoading) {
      form.setValue('modelOptions.allowModelSelection', data.allowModelSelection || false);
      form.setValue('modelOptions.defaultModelId', data.defaultModelId || 'GPT-4o');

      // Initialize models array with API data
      if (data.models) {
        const modelsWithSelection = data.models.map((model) => ({
          modelId: model.modelId,
          isSelected: false
        }));
        form.setValue('modelOptions.models', modelsWithSelection);
      }
    }
  }, [data, textmodelsLoading, form]);

  // Reset connected agents when type changes from Agent to something else
  useEffect(() => {
    if (watchType !== AssistantType.Agent && connectedAgents.length > 0) {
      setConnectedAgents([]);
      setSelectedAgentId('');
      setAgentName('');
      setAgentDescription('');
      form.resetField('agentConfiguration.bingConfig');
    }
  }, [watchType, connectedAgents.length]);

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
      const fileData = {
        ...values
      } as Assistant;

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
      // Enhanced error handling for different response statuses
      let errorMessage = 'An error occurred';

      if (error?.response) {
        const status = error.response.status;
        const data = error.response.data;

        if (status === 400) {
          // Handle validation errors from the API
          if (data?.error?.message) {
            // Handle OpenAI-style error format
            errorMessage = data.error.message;
          } else if (data?.errors) {
            // Handle validation errors
            const validationErrors = Object.values(data.errors).flat();
            errorMessage = validationErrors.join(', ');
          } else if (data?.message) {
            errorMessage = data.message;
          } else {
            errorMessage = 'Validation failed. Please check your input.';
          }
        } else if (status === 401) {
          errorMessage = 'Unauthorized. Please check your permissions.';
        } else if (status === 403) {
          errorMessage = 'Access denied. You do not have permission to perform this action.';
        } else if (status === 500) {
          errorMessage = 'Server error. Please try again later.';
        } else {
          errorMessage = `Request failed with status ${status}`;
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
                          <SelectItem value={AssistantType.Agent}>Agent</SelectItem>
                          <SelectItem value={AssistantType.Chat}>Chat</SelectItem>
                          <SelectItem value={AssistantType.Search}>Search</SelectItem>
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                {form.getValues('type') === 'Agent' && (
                  <div>
                    {/* Display connected agents count outside dialog */}
                    <div className="mb-4 p-3 bg-muted/30 rounded-lg border">
                      <div className="flex items-center gap-2 mb-2">
                        <Users className="h-4 w-4 text-muted-foreground flex-shrink-0" />
                        <span className="text-sm font-medium">
                          {connectedAgents.length} connected agent{connectedAgents.length !== 1 ? 's' : ''}
                        </span>
                      </div>
                      {connectedAgents.length > 0 && (
                        <div className="flex flex-wrap gap-1 mt-2">
                          {connectedAgents.map((agent) => (
                            <span
                              key={agent.agentId}
                              className="px-2 py-1 bg-primary/10 text-primary text-xs rounded-full break-words">
                              {agent.agentName}
                            </span>
                          ))}
                        </div>
                      )}
                    </div>

                    <BaseDialog
                      label={
                        <div className="flex items-center gap-2">
                          <Plus className="h-4 w-4" />
                          Configure connected agents
                        </div>
                      }
                      title="Connected Agents"
                      disabled={false}
                      description="Select and configure agents to connect with this assistant">
                      <div className="space-y-6">
                        <div>
                          <FormLabel className="text-sm font-medium">Agent *</FormLabel>
                          <Select value={selectedAgentId} onValueChange={setSelectedAgentId}>
                            <SelectTrigger>
                              <SelectValue placeholder="Select an agent" />
                            </SelectTrigger>
                            <SelectContent>
                              {agents?.map((agent) => (
                                <SelectItem key={agent.id} value={agent.agentConfiguration.agentId}>
                                  {agent.name}
                                </SelectItem>
                              ))}
                            </SelectContent>
                          </Select>
                        </div>

                        <div>
                          <FormLabel className="text-sm font-medium">Agent Unique Name *</FormLabel>
                          <Input
                            placeholder="agent_name (letters and underscores only)"
                            value={agentName}
                            onChange={(e) => setAgentName(e.target.value)}
                            pattern="^[a-zA-Z_]+$"
                          />
                          <p className="text-xs text-muted-foreground mt-1">
                            Must contain only letters and underscores
                          </p>
                        </div>

                        <div>
                          <FormLabel className="text-sm font-medium">Activation Description *</FormLabel>
                          <Textarea
                            placeholder="Describe when and how this agent should be activated..."
                            className="min-h-[100px] resize-none"
                            value={agentDescription}
                            onChange={(e) => setAgentDescription(e.target.value)}
                          />
                        </div>

                        <div className="flex justify-end gap-2 pt-4 border-t">
                          <Button
                            size="sm"
                            onClick={handleAddAgent}
                            disabled={!selectedAgentId || !agentName.trim() || !agentDescription.trim()}
                            className="w-full sm:w-auto">
                            <Plus className="h-4 w-4 mr-2" />
                            Add Agent
                          </Button>
                        </div>

                        {/* Display connected agents */}
                        {connectedAgents.length > 0 && (
                          <div className="pt-4 border-t">
                            <FormLabel className="text-sm font-medium mb-3 block">
                              Connected Agents ({connectedAgents.length})
                            </FormLabel>
                            <div className="max-h-60 overflow-y-auto space-y-3 pr-2">
                              {connectedAgents.map((agent) => (
                                <div
                                  key={agent.agentId}
                                  className="flex flex-col sm:flex-row sm:items-start sm:justify-between p-3 border rounded-lg bg-muted/50 gap-2">
                                  <div className="flex-1 min-w-0">
                                    <div className="font-medium text-sm break-words">{agent.agentName}</div>
                                    <div className="text-xs text-muted-foreground mt-1 break-words">
                                      {agent.activationDescription}
                                    </div>
                                  </div>
                                  <Button
                                    variant="ghost"
                                    size="sm"
                                    onClick={() => handleRemoveAgent(agent.agentId)}
                                    className="self-start sm:ml-2 h-8 w-8 p-0 text-destructive hover:text-destructive hover:bg-destructive/10 flex-shrink-0">
                                    <X className="h-3 w-3" />
                                  </Button>
                                </div>
                              ))}
                            </div>
                          </div>
                        )}
                      </div>
                    </BaseDialog>
                  </div>
                )}

                {watchType === AssistantType.Agent && (
                  <>
                    <FormField
                      control={form.control}
                      name="agentConfiguration.bingConfig.enableWebSearch"
                      render={({ field }) => (
                        <FormItem className="flex items-center space-y-0">
                          <FormLabel htmlFor="container-select" className="my-auto">
                            Enable Web Search
                          </FormLabel>
                          <FormControl>
                            <Checkbox className="p-3 ms-2" checked={field.value} onCheckedChange={field.onChange} />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                    {watchEnableWebSearch && (
                      <FormField
                        control={form.control}
                        name="agentConfiguration.bingConfig.webResultsCount"
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>Web Results Count</FormLabel>
                            <FormControl>
                              <Input
                                {...field}
                                type="number"
                                placeholder="Enter a number between 1 and 50"
                                min={1}
                                max={50}
                                onChange={(e) => field.onChange(Number(e.target.value))}
                              />
                            </FormControl>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                    )}
                  </>
                )}

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

                {watchType !== AssistantType.Agent && (
                  <>
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
                  </>
                )}

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

                {/* <Dialog open={agentDialogOpen} onOpenChange={setAgentDialogOpen}>
                  <DialogContent className="sm:max-w-[500px]">
                    <DialogHeader>
                      <DialogTitle>Add Connected Agent</DialogTitle>
                      <DialogDescription>
                        Specify the agent, a unique name, and activation details.
                      </DialogDescription>
                    </DialogHeader>

                    <div className="space-y-4 mt-2">
                     <div>
                        <label className="block mb-2 text-sm font-medium">Agent</label>
                        <Select value={selectedAgent} onValueChange={setSelectedAgent}>
                          <SelectTrigger>
                            <SelectValue placeholder="Select agent" />
                          </SelectTrigger>
                          <SelectContent>
                            {agentOptions.map(opt => (
                              <SelectItem key={opt.id} value={opt.id}>{opt.name}</SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                      </div>
                      <div>
                        <label className="block mb-2 text-sm font-medium">Detail the steps to activate the agent</label>
                        <textarea
                          className="w-full border rounded-md p-2 min-h-[80px]"
                          placeholder="Describe the specific conditions under which the agent should be activated in as much detail as possible"
                          value={activationDetail}
                          onChange={e => setActivationDetail(e.target.value)}
                        />
                      </div>
                    </div>

                    <DialogFooter>
                      <Button
                        type="button"
                        onClick={() => {
                          setConnectedAgents(prev => [
                            ...prev,
                            {
                              agentType: agentOptions.find(opt => opt.id === selectedAgent)?.name || selectedAgent,
                              description: (activationDetail ? ` - ${activationDetail}` : "")
                            }
                          ]);
                          setSelectedAgent('');
                          setActivationDetail('');
                          setAgentDialogOpen(false);
                        }}
                        disabled={!selectedAgent}
                        
                      >
                        Add Agent
                      </Button>
                      <Button
                        variant="outline"
                        type="button"
                        onClick={() => setAgentDialogOpen(false)}
                      >
                        Cancel
                      </Button>
                    </DialogFooter>
                  </DialogContent>
                </Dialog> */}

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
